// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Util;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two;

public abstract class Drawable2d : Drawable
{
    private Invalidation invalidatedFlags = Invalidation.All;

    [Resolved]
    private OpenGlRenderer renderer = null!;

    public float X
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.Geometry);
        }
    } = 0;

    public float Y
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.Geometry);
        }
    } = 0;

    public float Width
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.Geometry | Invalidation.Layout | Invalidation.Size);
            Parent?.Invalidate(Invalidation.Layout);
            invalidateChildrenIfComposite(Invalidation.Size | Invalidation.Geometry);
        }
    } = 0f;

    public float Height
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.Geometry | Invalidation.Layout | Invalidation.Size);
            Parent?.Invalidate(Invalidation.Layout);
            invalidateChildrenIfComposite(Invalidation.Size | Invalidation.Geometry);
        }
    } = 0f;

    public Vector2 Size
    {
        get => new Vector2(Width, Height);
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    public Vector2 Position
    {
        get => new Vector2(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    public float Rotation
    {
        get;
        set
        {
            if (Precision.IsSame(value, field)) return;
            if (!float.IsFinite(value)) throw new ArgumentException($@"{nameof(Rotation)} must be finite, but is {value}.", nameof(value));
            field = value;
            Invalidate(Invalidation.Geometry);
        }
    } = 0f;

    public Vector4 Margin
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Geometry | Invalidation.Size);
        }
    } = Vector4.Zero;

    public Anchor Origin
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    } = Anchor.TopLeft;

    public Anchor Anchor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    } = Anchor.TopLeft;


    public Axes RelativeSizeAxes
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Size | Invalidation.Layout);
        }
    } = Axes.None;


    public Axes FillRemainingAxes
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Size | Invalidation.Layout);
        }
    } = Axes.None;

    public Axes AutoSizeAxes
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Size | Invalidation.Layout);
        }
    } = Axes.None;

    public Vector2 Scale
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Geometry | Invalidation.Size);
        }
    } = Vector2.One;

    public Drawable2d? Parent
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (RelativeSizeAxes != Axes.None || FillRemainingAxes != Axes.None)
                Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    } = null;

    public bool IsHovered { get; set; } = false;

    public bool IsMouseDown { get; set; } = false;

    public bool IsLoaded => LoadState >= DrawableLoadState.Loaded;

    protected virtual bool AcceptsInput { get; } = true;

    public bool CanHandleInput => IsLoaded && AcceptsInput;

    public Vector2 InheritedScale => Parent == null ? Scale : Parent.InheritedScale * Scale;

    protected float InheritedAlpha => Alpha * (Parent?.InheritedAlpha ?? 1f);

    public Vector2 ScreenSpacePosition
    {
        get
        {
            var parentScale = Parent?.InheritedScale ?? Vector2.One;

            var anchorPos = Vector2.Zero;
            if (Parent != null)
            {
                anchorPos = Parent.ScreenSpacePosition + getAnchorOffset(Parent.Size, Anchor) * parentScale;
            }

            var posOffset = (Position + getMarginOffset()) * parentScale;
            var originOffset = getAnchorOffset(Size, Origin) * InheritedScale;

            return anchorPos + posOffset + getMarginOffset() - originOffset;
        }
    }

    public void Invalidate(Invalidation flags)
    {
        if ((invalidatedFlags & flags) == flags) return;

        invalidatedFlags |= flags;
        EngineStatistics.LAYOUT_INVALIDATIONS.Increment();

        if ((flags & Invalidation.Geometry) != Invalidation.None)
        {
            // if (this is CompositeDrawable2d composite)
            // {
            // for (int i = 0; i < composite.InternalChildren.Count; i++)
            // composite.InternalChildren[i].Invalidate(Invalidation.Geometry);
            // }
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
            : Width;

        var targetHeight = RelativeSizeAxes.HasFlagFast(Axes.Y)
            ? Parent.Size.Y - Margin.Y - Margin.W
            : Height;

        // Use size setter only if values actually changed to
        // avoid unnecessary child invalidations
        if (!Precision.IsSame(targetWidth, Width) || !Precision.IsSame(targetHeight, Height))
        {
            Size = new Vector2(targetWidth, targetHeight);
        }
    }

    protected abstract void OnDraw2d();

    protected internal override void OnDraw()
    {
        if (!Visible || InheritedAlpha <= 0.001f || !IsLoaded) return;

        beginLocalSpace();

        try
        {
            OnDraw2d();
        }
        finally
        {
            endLocalSpace();
            renderer.EndShader();
        }
    }

    private void beginLocalSpace()
    {
        renderer.PushMatrix();

        var anchorPos = Vector2.Zero;
        if (Parent != null) anchorPos = getAnchorOffset(Parent.Size, Anchor);

        var originOffset = getAnchorOffset(Size, Origin);

        renderer.Translate(anchorPos.X + Position.X + Margin.X, anchorPos.Y + Position.Y + Margin.Y, 0);

        if (Rotation != 0f) renderer.RotateAround(new Vector2(Width / 2, Height / 2), Rotation);

        renderer.Scale(Scale.X, Scale.Y, 1);
        renderer.Translate(-originOffset.X, -originOffset.Y, 0);
        renderer.Translate(-Margin.X, -Margin.Y, 0);
    }

    public Vector2 GetScreenSpaceCenter()
    {
        var screenPos = ScreenSpacePosition;
        var scaledSize = Size * InheritedScale;
        return screenPos + scaledSize / 2f;
    }

    private void endLocalSpace()
    {
        renderer.PopMatrix();
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

            _ => Vector2.Zero,
        };
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

    private void invalidateChildrenIfComposite(Invalidation invalidation)
    {
        if(this is CompositeDrawable2d composite) composite.InvalidateChildren(invalidation);
    }
}
