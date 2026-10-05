// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Graphics.Three;

public abstract class Drawable3D : Drawable
{
    private Invalidation invalidatedFlags = Invalidation.All;

    public float X
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.Geometry);
            InvalidateIfCamera(Invalidation.CameraView);
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
            InvalidateIfCamera(Invalidation.CameraView);
        }
    } = 0;

    public float Z
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.Geometry);
            InvalidateIfCamera(Invalidation.CameraView);
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
            // Parent?.Invalidate(Invalidation.Layout);
            // invalidateChildrenIfComposite(Invalidation.Size | Invalidation.Geometry);
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
            // Parent?.Invalidate(Invalidation.Layout);
            // invalidateChildrenIfComposite(Invalidation.Size | Invalidation.Geometry);
        }
    } = 0f;

    public float Length
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.Geometry | Invalidation.Layout | Invalidation.Size);
            // Parent?.Invalidate(Invalidation.Layout);
            // invalidateChildrenIfComposite(Invalidation.Size | Invalidation.Geometry);
        }
    } = 0f;

    public Vector3 Size
    {
        get => new Vector3(Width, Height, Length);
        set
        {
            Width = value.X;
            Height = value.Y;
            Length = value.Z;
        }
    }

    public Vector3 Position
    {
        get => new Vector3(X, Y, Z);
        set
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
        }
    }

    //TODO rotation xyz


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

    public bool Visible {
        get;
        set
        {
            if(field == value) return;
            field = value;

            Invalidate(Invalidation.Size | Invalidation.Layout | Invalidation.Size | Invalidation.CameraView);
        }

    } = true;

    public Drawable3D? Parent
    {
        get;
        set;
    } = null;

    public bool IsLoaded => LoadState >= DrawableLoadState.Loaded;

    public bool ShouldBeDrawn => Visible && IsLoaded && Alpha > 0;

    public Vector2 InheritedScale => Parent == null ? Scale : Parent.InheritedScale * Scale;

    protected float InheritedAlpha => Alpha * (Parent?.InheritedAlpha ?? 1f);


    public void Invalidate(Invalidation flags)
    {
        if ((invalidatedFlags & flags) == flags) return;

        invalidatedFlags |= flags;
        DrawStatistics.Increment(DrawStatistics.Type.Invalidations);

        // if ((flags & Invalidation.Geometry) != Invalidation.None)
        // {
        // if (this is CompositeDrawable2D composite)
        // {
        // for (int i = 0; i < composite.InternalChildren.Count; i++)
        // composite.InternalChildren[i].Invalidate(Invalidation.Geometry);
        // }
        // }

        // if ((flags & Invalidation.Size) != Invalidation.None)
        // {
        // Parent?.Invalidate(Invalidation.Size);
        // }
    }

    protected void UpdateLayout()
    {
        if (invalidatedFlags == Invalidation.None) return;

        var dirty = invalidatedFlags;
        invalidatedFlags = Invalidation.None;

        OnLayout(dirty);
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        UpdateLayout();
        if (Animator.IsValueCreated)
        {
            Animator.Value.Update(frameInfo);
        }
    }

    protected virtual void OnLayout(Invalidation dirty)
    {
        // if (dirty.HasFlagFast(Invalidation.Size))
        // {
            // UpdateRelativeSize();
        // }

        // if (dirty.HasFlagFast(Invalidation.DrawNode))
        // {
            // CachedBorderColor = BorderColor.ToMatrix4();
        // }
    }

    protected abstract void OnDraw3D();

    protected override void InternalLoadComplete()
    {
        Invalidate(Invalidation.All);
        UpdateLayout();
    }

    protected internal override void OnDraw()
    {
        if (!Visible || InheritedAlpha <= 0.001f || !IsLoaded) return;

        updateDrawMatrix();
        OnDraw3D();
    }

    private void updateDrawMatrix()
    {
        DrawMatrix.Reset();

        if (Parent != null)
        {
            DrawMatrix.Matrix = Parent.DrawMatrix.Matrix;
            DrawMatrix.InverseMatrix = Parent.DrawMatrix.InverseMatrix;
        }

        // var anchorPos = Parent != null ? GetAnchorOffset(Parent.Size, Anchor) : Vector2.Zero;
        // DrawMatrix.Translate(anchorPos.X + Position.X + Margin.X, anchorPos.Y + Position.Y + Margin.Y, 0);
        DrawMatrix.Translate(X, Y, Z);

        DrawMatrix.Scale(Scale.X, Scale.Y, 1f);
        // if (Rotation != 0) DrawMatrix.Rotate(Rotation, 0, 0, 1);

        // var originOffset = GetAnchorOffset(Size, Origin);
        // DrawMatrix.Translate(-originOffset.X - Margin.X, -originOffset.Y - Margin.Y, 0);
    }

    protected void InvalidateIfCamera(Invalidation invalidation)
    {
        if(this is not Camera) return;
        Invalidate(invalidation);
    }

}
