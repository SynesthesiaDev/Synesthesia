using System.Numerics;
using Raylib_cs;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;

namespace Synesthesia.Engine.Graphics.Three;

public abstract class Drawable3d : Drawable
{
    public Vector3 Position { get; set; } = new(0, 0, 0);

    public float Width = 1f;

    public float Height = 1f;

    public float Length = 1f;

    public Vector3 Size
    {
        get => new(Width, Height, Length);
        set
        {
            Width = value.X;
            Height = value.Y;
            Length = value.Z;
        }
    }

    public Vector3 Scale { get; set; } = new(1);

    protected float InheritedAlpha => Alpha * (Parent?.InheritedAlpha ?? 1f);

    public Vector3 WorldScale => Parent == null ? Scale : Parent.WorldScale * Scale;

    public Quaternion LocalRotationQuaternion => Quaternion.CreateFromYawPitchRoll(
        Rotation.Y * (MathF.PI / 180f),
        Rotation.X * (MathF.PI / 180f),
        Rotation.Z * (MathF.PI / 180f)
    );

    public Quaternion WorldRotationQuaternion =>
        Parent == null
            ? LocalRotationQuaternion
            : Quaternion.Normalize(Parent.WorldRotationQuaternion * LocalRotationQuaternion);

    public Vector3 WorldPosition
    {
        get
        {
            if (Parent == null) return Position;

            var scaledLocalOffset = Position * Parent.WorldScale;

            var rotatedLocalOffset = Vector3.Transform(scaledLocalOffset, Parent.WorldRotationQuaternion);

            return Parent.WorldPosition + rotatedLocalOffset;
        }
    }

    public Drawable3d? Parent { get; set; }

    protected virtual bool DirectDraw { get; set; } = false;

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if (Animator.IsValueCreated) Animator.Value.Update(frameInfo);
    }


    protected internal sealed override void OnDraw()
    {
        if (DirectDraw)
        {
            OnDraw3d();
            return;
        }

        if (!Visible || InheritedAlpha <= 0.001f) return;

        Raylib.BeginBlendMode(BlendMode);

        beginLocalSpace();
        try
        {
            OnDraw3d();
        }
        finally
        {
            Raylib.EndBlendMode();
            endLocalSpace();
            // Raylib.EndShaderMode();
        }
    }

    protected abstract void OnDraw3d();

    private void beginLocalSpace()
    {
        Rlgl.PushMatrix();

        Rlgl.Translatef(Position.X, Position.Y, Position.Z);

        Rlgl.Scalef(Scale.X, Scale.Y, Scale.Z);

        if (Rotation.X != 0) Rlgl.Rotatef(Rotation.X, 1f, 0f, 0f);
        if (Rotation.Y != 0) Rlgl.Rotatef(Rotation.Y, 0f, 1f, 0f);
        if (Rotation.Z != 0) Rlgl.Rotatef(Rotation.Z, 0f, 0f, 1f);

        Rlgl.Scalef(Size.X, Size.Y, Size.Z);
    }

    public Animation<T> TransformTo<T>(string field, T startValue, T endValue, long duration, Easing easing, Transform<T> transform, Action<T> onUpdate, Action? onComplete = null, long delay = 0L)
    {
        var animation = new Animation<T>
        {
            StartValue = startValue,
            EndValue = endValue,
            Duration = duration,
            Transform = transform,
            Easing = easing,
            OnUpdate = onUpdate,
            OnComplete = onComplete,
            Delay = delay
        };
        Animator.Value.AddAnimation(field, animation);
        return animation;
    }

    protected override void Dispose(bool isDisposing)
    {
        if (Animator.IsValueCreated) Animator.Value.Dispose();
        Parent = null;
        base.Dispose(isDisposing);
    }

    private void endLocalSpace()
    {
        Rlgl.PopMatrix();
    }
}
