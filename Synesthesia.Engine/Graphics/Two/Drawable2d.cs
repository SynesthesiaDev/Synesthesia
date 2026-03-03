using System.Numerics;
using Common.Statistics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Shaders;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Resources;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two;

public abstract class Drawable2d : Drawable
{
    private Invalidation invalidatedFlags = Invalidation.All;
    private Axes fillRemainingAxes = Axes.None;
    private Axes autoSizeAxes = Axes.None;
    private Axes relativeSizeAxes = Axes.None;
    private Anchor origin = Anchor.TopLeft;
    private Anchor anchor = Anchor.TopLeft;
    private Vector2 position = new(0, 0);
    private Vector2 scale = new(1);
    private Vector4 margin = new(0);
    private Drawable2d? parent;
    private float width;
    private float height;

    [Resolved]
    private IResourceStore<Shader> shaderStore = null!;

    private Shader? alphaShader;

    public Vector2 Position
    {
        get => position;
        set
        {
            if (position == value) return;
            position = value;
            Invalidate(Invalidation.Geometry);
        }
    }

    public Anchor Anchor
    {
        get => anchor;
        set
        {
            if (anchor == value) return;
            anchor = value;
            Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    }

    public Anchor Origin
    {
        get => origin;
        set
        {
            if (origin == value) return;
            origin = value;
            Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    }

    public Axes AutoSizeAxes
    {
        get => autoSizeAxes;
        set
        {
            if (autoSizeAxes == value) return;
            autoSizeAxes = value;
            Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    }

    public Axes FillRemainingAxes
    {
        get => fillRemainingAxes;
        set
        {
            if (fillRemainingAxes == value) return;
            fillRemainingAxes = value;
            Invalidate(Invalidation.Layout);
        }
    }

    public Axes RelativeSizeAxes
    {
        get => relativeSizeAxes;
        set
        {
            if (relativeSizeAxes == value) return;
            relativeSizeAxes = value;
            Invalidate(Invalidation.Size | Invalidation.Layout);
        }
    }

    protected override void InternalLoadComplete()
    {
        Invalidate(Invalidation.All);
        UpdateLayout();
    }

    public float Height
    {
        get => height;
        set
        {
            if (Precision.IsSame(height, value)) return;
            height = value;
            Invalidate(Invalidation.Geometry | Invalidation.Layout | Invalidation.Size);
            parent?.Invalidate(Invalidation.Layout);
            invalidateChildrenIfComposite(Invalidation.Size | Invalidation.Geometry);
        }
    }

    public float Width
    {
        get => width;
        set
        {
            if (Precision.IsSame(width, value)) return;
            width = value;
            Invalidate(Invalidation.Geometry | Invalidation.Layout | Invalidation.Size);
            parent?.Invalidate(Invalidation.Layout);
            invalidateChildrenIfComposite(Invalidation.Size | Invalidation.Geometry);
        }
    }

    private void invalidateChildrenIfComposite(Invalidation flags)
    {
        if (this is CompositeDrawable2d compositeDrawable2d) compositeDrawable2d.InvalidateChildren(flags);
    }

    public Vector2 Size
    {
        get => new(Width, Height);
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    public Vector2 Scale
    {
        get => scale;
        set
        {
            if (scale == value) return;
            scale = value;
            Invalidate(Invalidation.Geometry | Invalidation.Size);
        }
    }

    public Vector4 Margin
    {
        get => margin;
        set
        {
            if (margin == value) return;
            margin = value;
            Invalidate(Invalidation.Geometry | Invalidation.Size);
        }
    }

    public Drawable2d? Parent
    {
        get => parent;
        set
        {
            if (parent == value) return;
            parent = value;
            if (RelativeSizeAxes != Axes.None || FillRemainingAxes != Axes.None)
                Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    }

    public bool IsHovered { get; set; } = false;

    public bool IsMouseDown { get; set; } = false;

    protected virtual bool AcceptsInput { get; } = true;

    public bool CanHandleInput => IsLoaded && AcceptsInput;

    public Vector2 InheritedScale => Parent == null ? Scale : Parent.InheritedScale * Scale;

    public Vector2 ScreenSpacePosition
    {
        get
        {
            var parentScale = parent?.InheritedScale ?? Vector2.One;

            var anchorPos = Vector2.Zero;
            if (parent != null)
            {
                anchorPos = parent.ScreenSpacePosition + getAnchorOffset(parent.Size, Anchor) * parentScale;
            }

            var posOffset = (Position + getMarginOffset()) * parentScale;
            var originOffset = getAnchorOffset(Size, Origin) * InheritedScale;

            return anchorPos + posOffset + getMarginOffset() - originOffset;
        }
    }

    public bool Contains(Vector2 screenSpacePoint)
    {
        if (!Visible) return false;

        var pos = ScreenSpacePosition;
        var scaledSize = Size * InheritedScale;

        return screenSpacePoint.X >= pos.X && screenSpacePoint.X <= pos.X + scaledSize.X &&
               screenSpacePoint.Y >= pos.Y && screenSpacePoint.Y <= pos.Y + scaledSize.Y;
    }

    public Vector2 ToLocalSpace(Vector2 screenSpacePoint)
    {
        if (Parent == null) return screenSpacePoint - Position;

        var pointInParentSpace = Parent.ToLocalSpace(screenSpacePoint);

        // offset applied in matrix4
        var anchorOffset = getAnchorOffset(Parent.Size, Anchor);
        var originOffset = getAnchorOffset(Size, Origin);

        var localPoint = (pointInParentSpace - (anchorOffset + Position + new Vector2(Margin.X, Margin.Y))) + originOffset;

        return localPoint / Scale;
    }

    private Vector2 getMarginOffset()
    {
        var x = Anchor.HasFlagFast(Anchor.Left) ? Margin.X : (Anchor.HasFlagFast(Anchor.Right) ? -Margin.Z : 0);
        var y = Anchor.HasFlagFast(Anchor.Top) ? Margin.Y : (Anchor.HasFlagFast(Anchor.Bottom) ? -Margin.W : 0);

        if (Anchor is Anchor.Centre or Anchor.TopCentre or Anchor.BottomCentre)
            x = (Margin.X - Margin.Z) / 2f;
        if (Anchor is Anchor.Centre or Anchor.CentreLeft or Anchor.CentreRight)
            y = (Margin.Y - Margin.W) / 2f;

        return new Vector2(x, y);
    }

    protected float InheritedAlpha => Alpha * (Parent?.InheritedAlpha ?? 1f);

    protected internal virtual bool OnHover(MouseMoveInputEvent e)
    {
        return false;
    }

    protected internal virtual void OnHoverLost(MouseMoveInputEvent e)
    {
    }

    protected internal virtual bool OnMouseDown(PointInput e)
    {
        return false;
    }

    protected internal virtual void OnMouseUp(PointInput e)
    {
    }

    protected internal virtual bool OnKeyDown(KeyboardKey e)
    {
        return false;
    }

    protected internal virtual void OnKeyUp(KeyboardKey e)
    {
    }

    protected internal virtual void OnMouseUp(KeyboardKey e)
    {
    }

    protected internal virtual bool OnActionBindingDown(ActionBinding e)
    {
        return false;
    }

    protected internal virtual void OnActionBindingUp(ActionBinding e)
    {
    }

    protected internal virtual bool OnMouseWheel(float delta)
    {
        return false;
    }

    public void Invalidate(Invalidation flags)
    {
        if ((invalidatedFlags & flags) == flags) return;

        invalidatedFlags |= flags;
        EngineStatistics.LAYOUT_INVALIDATIONS.Increment();

        if ((flags & Invalidation.Geometry) != 0)
        {
            if (this is CompositeDrawable2d composite)
            {
                for (int i = 0; i < composite.InternalChildren.Count; i++)
                    composite.InternalChildren[i].Invalidate(Invalidation.Geometry);
            }
        }

        if ((flags & Invalidation.Size) != 0)
        {
            Parent?.Invalidate(Invalidation.Size);
        }
    }

    protected internal void UpdateLayout()
    {
        var dirty = invalidatedFlags;
        invalidatedFlags = Invalidation.None;

        if (dirty == Invalidation.None) return;

        OnLayout(dirty);
    }

    protected virtual void OnLayout(Invalidation dirty)
    {
        if (dirty.HasFlagFast(Invalidation.Size))
        {
            UpdateRelativeSize();
        }
    }

    protected virtual void UpdateRelativeSize()
    {
        if (Parent == null) return;

        var targetWidth = RelativeSizeAxes.HasFlagFast(Axes.X)
            ? Parent.Size.X - Margin.X - Margin.Z
            : width;

        var targetHeight = RelativeSizeAxes.HasFlagFast(Axes.Y)
            ? Parent.Size.Y - Margin.Y - Margin.W
            : height;

        // Use size setter only if values actually changed to avoid
        // triggering unnecessary child invalidations
        if (!Precision.IsSame(targetWidth, width) || !Precision.IsSame(targetHeight, height))
        {
            Size = new Vector2(targetWidth, targetHeight);
        }
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        UpdateLayout();

        if (Animator.IsValueCreated)
        {
            Animator.Value.Update(frameInfo);
        }
    }

    //TODO proper shader caching
    private static int alphaUniformLoc = -1;

    protected internal sealed override void OnDraw()
    {
        if (!Visible || InheritedAlpha <= 0.001f || !IsLoaded) return;

        alphaShader ??= shaderStore.Get("Synesthesia.Resources.Shaders.alpha.fsh");

        if (alphaUniformLoc == -1)
            alphaUniformLoc = Raylib.GetShaderLocation(alphaShader.NativeShader, "alpha");

        Raylib.SetShaderValue(alphaShader.NativeShader, alphaUniformLoc, InheritedAlpha, ShaderUniformDataType.Float);

        Raylib.BeginShaderMode(alphaShader.NativeShader);
        Raylib.BeginBlendMode(BlendMode.Alpha);

        beginLocalSpace();

        try
        {
            OnDraw2d();
        }
        finally
        {
            endLocalSpace();
            Raylib.EndBlendMode();
            Raylib.EndShaderMode();
        }
    }

    public Vector2 GetScreenSpaceCenter()
    {
        var screenPos = ScreenSpacePosition;
        var scaledSize = Size * InheritedScale;
        return screenPos + scaledSize / 2f;
    }

    protected abstract void OnDraw2d();

    private void beginLocalSpace()
    {
        Rlgl.PushMatrix();

        var anchorPos = Vector2.Zero;
        if (Parent != null)
        {
            anchorPos = getAnchorOffset(Parent.Size, Anchor);
        }

        var originOffset = getAnchorOffset(Size, Origin);

        Rlgl.Translatef(anchorPos.X + Position.X + Margin.X, anchorPos.Y + Position.Y + Margin.Y, 0);

        if (Rotation.Z != 0) Rlgl.Rotatef(Rotation.Z, 0, 0, 1);

        Rlgl.Scalef(Scale.X, Scale.Y, 1);

        Rlgl.Translatef(-originOffset.X, -originOffset.Y, 0);
        Rlgl.Translatef(-Margin.X, -Margin.Y, 0);
    }

    private void endLocalSpace()
    {
        Rlgl.PopMatrix();
    }

    private static Vector2 getAnchorOffset(Vector2 size, Anchor anchor)
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

    public Animation<Vector2> MoveTo(Vector2 newPosition, long duration, Easing easing)
    {
        return TransformTo(nameof(Position), Position, newPosition, duration, easing, Transforms.VECTOR2, vec => { Position = vec; });
    }

    public Animation<Vector2> ScaleTo(float newScale, long duration, Easing easing)
    {
        return ScaleTo(new Vector2(newScale), duration, easing);
    }

    public Animation<Vector2> ScaleFromTo(float oldScale, float newScale, long duration, Easing easing)
    {
        return ScaleFromTo(new Vector2(oldScale), new Vector2(newScale), duration, easing);
    }

    public Animation<float> ResizeWidthTo(float newWidth, long duration, Easing easing)
    {
        return TransformTo(nameof(Width), Width, newWidth, duration, easing, Transforms.FLOAT, a => Width = a);
    }

    public Animation<float> ResizeHeightTo(float newHeight, long duration, Easing easing)
    {
        return TransformTo(nameof(Height), Height, newHeight, duration, easing, Transforms.FLOAT, a => Height = a);
    }

    public Animation<Vector2> ScaleTo(Vector2 newScale, long duration, Easing easing)
    {
        return TransformTo(nameof(Scale), Scale, newScale, duration, easing, Transforms.VECTOR2, vec => { Scale = vec; });
    }

    public Animation<Vector2> ScaleFromTo(Vector2 oldScale, Vector2 newScale, long duration, Easing easing)
    {
        return TransformTo(nameof(Scale), oldScale, newScale, duration, easing, Transforms.VECTOR2, vec => { Scale = vec; });
    }


    public Animation<Vector2> ResizeTo(Vector2 newSize, long duration, Easing easing)
    {
        return TransformTo(nameof(Size), Size, newSize, duration, easing, Transforms.VECTOR2, vec => { Size = vec; });
    }

    public Animation<Vector3> RotateTo(Vector3 newRotation, long duration, Easing easing)
    {
        return TransformTo(nameof(Rotation), Rotation, newRotation, duration, easing, Transforms.VECTOR3, vec => { Rotation = vec; });
    }

    public Animation<float> FadeTo(float newAlpha, long duration, Easing easing)
    {
        return TransformTo(nameof(Alpha), Alpha, newAlpha, duration, easing, Transforms.FLOAT, a => Alpha = a);
    }

    public Animation<float> FadeFromTo(float startAlpha, float endAlpha, long duration, Easing easing)
    {
        return TransformTo(nameof(Alpha), startAlpha, endAlpha, duration, easing, Transforms.FLOAT, a => Alpha = a);
    }

    protected override void Dispose(bool isDisposing)
    {
        if (Animator.IsValueCreated) Animator.Value.Dispose();
        Parent = null;
        base.Dispose(isDisposing);
    }

    public record PointInput(IInputEvent Event, Vector2 MousePosition, bool IsDown);
}
