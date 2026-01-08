using System.Numerics;
using Common.Logger;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Threading.Runners;

namespace Synesthesia.Engine.Graphics.Two;

public abstract class Drawable2d : Drawable
{
    public Vector2 Position { get; set; } = new(0, 0);

    public Anchor Anchor { get; set; } = Anchor.TopLeft;

    public Anchor Origin { get; set; } = Anchor.TopLeft;

    public Axes AutoSizeAxes { get; set; } = Axes.None;

    public Axes RelativeSizeAxes { get; set; } = Axes.None;

    public Vector2 Size { get; set; } = new(1);

    public Vector2 Scale { get; set; } = new(1);

    public Vector4 Margin { get; set; } = new(0);

    public Drawable2d? Parent { get; set; }

    public bool IsHovered { get; set; } = false;

    public bool IsMouseDown { get; set; } = false;

    public long Depth = 0;

    public Vector2 ScreenSpacePosition
    {
        get
        {
            var anchorPos = Vector2.Zero;
            if (Parent != null)
            {
                anchorPos = Parent.ScreenSpacePosition + GetAnchorOffset(Parent.Size, Anchor);
            }

            var originOffset = GetAnchorOffset(Size, Origin) * Scale;
            return anchorPos + Position - originOffset;
        }
    }

    public bool Contains(Vector2 screenSpacePoint)
    {
        if (!Visible) return false;

        var pos = ScreenSpacePosition;
        var scaledSize = Size * Scale;

        return screenSpacePoint.X >= pos.X && screenSpacePoint.X <= pos.X + scaledSize.X &&
               screenSpacePoint.Y >= pos.Y && screenSpacePoint.Y <= pos.Y + scaledSize.Y;
    }

    public Vector2 ToLocalSpace(Vector2 screenSpacePoint)
    {
        if (Parent == null) return screenSpacePoint - Position;

        var pointInParentSpace = Parent.ToLocalSpace(screenSpacePoint);

        // offset applied in matrix4
        var anchorOffset = GetAnchorOffset(Parent.Size, Anchor);
        var originOffset = GetAnchorOffset(Size, Origin);

        var localPoint = (pointInParentSpace - (anchorOffset + Position + new Vector2(Margin.X, Margin.Y))) +
                         originOffset;

        return localPoint / Scale;
    }

    // for properly applying alpha to all children, multiply local alpha by parent's inherited alpha recursively
    protected float InheritedAlpha => Alpha * Parent?.InheritedAlpha ?? Alpha;

    protected internal virtual bool OnHover(HoverEvent e)
    {
        return false;
    }

    protected internal virtual void OnHoverLost(HoverEvent e)
    {
    }

    protected internal virtual bool OnMouseDown(MouseEvent e)
    {
        return false;
    }

    protected internal virtual void OnMouseUp(MouseEvent e)
    {
    }

    protected internal override void OnUpdate()
    {
        if (RelativeSizeAxes != Axes.None && AutoSizeAxes != Axes.None)
        {
            throw new InvalidOperationException("Cannot have both 'AutoSizeAxis' and 'RelativeSizeAxes'");
        }

        if (Parent != null)
        {
            Size = RelativeSizeAxes switch
            {
                Axes.X => Size with { X = Parent.Size.X - Margin.X - Margin.Z },
                Axes.Y => Size with { Y = Parent.Size.Y - Margin.Y - Margin.W },
                Axes.Both => Size with { X = Parent.Size.X - Margin.X - Margin.Z } with
                {
                    Y = Parent.Size.Y - Margin.Y - Margin.W
                },
                _ => Size
            };
        }
    }

    //TODO proper shader caching
    private static int _alphaUniformLoc = -1;

    protected internal sealed override void OnDraw()
    {
        if (!Visible || InheritedAlpha <= 0.001f) return; // Skip if effectively invisible
        if (_alphaUniformLoc == -1)
            _alphaUniformLoc = Raylib.GetShaderLocation(RenderThreadRunner.AlphaShader, "alpha");

        Raylib.SetShaderValue(RenderThreadRunner.AlphaShader, _alphaUniformLoc, InheritedAlpha,
            ShaderUniformDataType.Float);
        Raylib.BeginShaderMode(RenderThreadRunner.AlphaShader);
        Raylib.BeginBlendMode(BlendMode.Alpha);

        BeginLocalSpace();

        try
        {
            OnDraw2d();
        }
        finally
        {
            EndLocalSpace();
            Raylib.EndBlendMode();
            Raylib.EndShaderMode();
        }
    }

    //TODO layout invalidation and update only when needed
    private void updateLayout()
    {
    }

    protected abstract void OnDraw2d();

    private void BeginLocalSpace()
    {
        Rlgl.PushMatrix();

        var anchorPos = Vector2.Zero;
        if (Parent != null)
        {
            anchorPos = GetAnchorOffset(Parent.Size, Anchor);
        }

        var originOffset = GetAnchorOffset(Size, Origin);

        Rlgl.Translatef(anchorPos.X + Position.X + Margin.X, anchorPos.Y + Position.Y + Margin.Y, 0);

        if (Rotation.Z != 0) Rlgl.Rotatef(Rotation.Z, 0, 0, 1);

        Rlgl.Scalef(Scale.X, Scale.Y, 1);

        Rlgl.Translatef(-originOffset.X, -originOffset.Y, 0);
    }

    private void EndLocalSpace()
    {
        Rlgl.PopMatrix();
    }

    private static Vector2 GetAnchorOffset(Vector2 size, Anchor anchor)
    {
        return anchor switch
        {
            Anchor.TopLeft => new Vector2(0, 0),
            Anchor.TopCentre => new Vector2(size.X / 2f, 0),
            Anchor.TopRight => new Vector2(size.X, 0),

            Anchor.CentreLeft => new Vector2(0, size.Y / 2f),
            Anchor.Centre => new Vector2(size.X / 2f, size.Y / 2f),
            Anchor.CentreRight => new Vector2(size.X, size.Y / 2f),

            Anchor.BottomLeft => new Vector2(0, size.Y),
            Anchor.BottomCentre => new Vector2(size.X / 2f, size.Y),
            Anchor.BottomRight => new Vector2(size.X, size.Y),

            _ => Vector2.Zero
        };
    }

    public Animation<T> TransformTo<T>
    (
        string field,
        T startValue,
        T endValue,
        long duration,
        Easing easing,
        Transform<T> transform,
        Action<T> onUpdate,
        Action<T>? onComplete = null,
        long delay = 0L
    )
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
        AnimationManager.AddAnimation(field, animation);
        return animation;
    }

    public Animation<Vector2> ScaleTo(float newScale, long duration, Easing easing)
    {
        return ScaleTo(new Vector2(newScale), duration, easing);
    }

    public Animation<Vector2> ScaleTo(Vector2 newScale, long duration, Easing easing)
    {
        return TransformTo
        (
            nameof(Scale),
            Scale,
            newScale,
            duration,
            easing,
            Transforms.Vector2,
            vec => { Scale = vec; }
        );
    }

    public Animation<Vector3> RotateTo(Vector3 newRotation, long duration, Easing easing)
    {
        return TransformTo
        (
            nameof(Rotation),
            Rotation,
            newRotation,
            duration,
            easing,
            Transforms.Vector3,
            vec => { Rotation = vec; }
        );
    }


    public Animation<float> FadeTo(float newAlpha, long duration, Easing easing)
    {
        return TransformTo
        (
            nameof(Alpha),
            Alpha,
            newAlpha,
            duration,
            easing,
            Transforms.Float,
            a => Alpha = a
        );
    }

    protected override void Dispose(bool isDisposing)
    {
        if (AnimationManager == null)
        {
            Logger.Warning($"AnimationManager was null when disposing {GetType().Name}");
        }
        else
        {
            AnimationManager.Dispose();
        }

        Parent = null;
        base.Dispose(isDisposing);
    }

    public record HoverEvent(bool Hovered, Vector2 MousePosition);

    public record MouseEvent(MouseButton Button, Vector2 MousePosition);
}