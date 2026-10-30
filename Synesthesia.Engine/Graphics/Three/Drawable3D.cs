// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.CompilerServices;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Input.ActionBindings;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Graphics.Three;

public abstract class Drawable3D : Drawable
{
    private Invalidation invalidatedFlags = Invalidation.All;

    [Singleton]
    protected Renderer3D Renderer = null!;

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
            Parent?.Invalidate(Invalidation.Layout);
            invalidateChildrenIfComposite(Invalidation.Size | Invalidation.Geometry);
        }
    } = 1f;

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
    } = 1f;

    public float Length
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
    } = 1f;

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

    public Vector3 Scale
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Geometry | Invalidation.Size);
        }
    } = Vector3.One;

    public bool Visible
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            Invalidate(Invalidation.Layout | Invalidation.Size | Invalidation.CameraView);
        }
    } = true;

    public Quaternion RawRotation
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Layout | Invalidation.Geometry);
        }
    }

    public Vector3 Rotation
    {
        get;
        set
        {
            field = value;

            float pitchRad = value.X * (MathF.PI / 180f);
            float yawRad = value.Y * (MathF.PI / 180f);
            float rollRad = value.Z * (MathF.PI / 180f);

            RawRotation = Quaternion.CreateFromYawPitchRoll(yawRad, pitchRad, rollRad);
        }
    }

    public Drawable3D? Parent
    {
        get;
        set;
    }

    public bool IsLoaded => LoadState >= DrawableLoadState.Loaded;

    public bool ShouldBeDrawn => Visible && IsLoaded && Alpha > 0;

    public Vector3 InheritedScale => Parent == null ? Scale : Parent.InheritedScale * Scale;

    public bool IsMouseDown { get; set; } = false;

    protected float InheritedAlpha => Alpha * (Parent?.InheritedAlpha ?? 1f);

    protected virtual bool AcceptsInput { get; } = true;

    public bool CanHandleInput => IsLoaded && AcceptsInput;

    public void Invalidate(Invalidation flags)
    {
        if ((invalidatedFlags & flags) == flags) return;

        invalidatedFlags |= flags;
        DrawStatistics.Increment(DrawStatistics.Type.Invalidations);

        if ((flags & Invalidation.Geometry) != Invalidation.None)
        {
            if (this is CompositeDrawable3D composite)
            {
                for (int i = 0; i < composite.InternalChildren.Count; i++)
                    composite.InternalChildren[i].Invalidate(Invalidation.Geometry);
            }
        }

        if ((flags & Invalidation.Size) != Invalidation.None)
        {
            Parent?.Invalidate(Invalidation.Size);
        }
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
    }

    protected abstract void Collect(RenderQueue renderQueue);

    protected override void InternalLoadComplete()
    {
        Invalidate(Invalidation.All);
        UpdateLayout();
    }

    protected internal override void OnDraw()
    {
        if (!Visible || InheritedAlpha <= 0.001f || !IsLoaded) return;

        UpdateDrawMatrix();
        Collect(Renderer.RenderQueue);
    }

    protected virtual void UpdateDrawMatrix()
    {
        DrawMatrix.Reset();

        if (Parent != null)
        {
            DrawMatrix.Matrix = Parent.DrawMatrix.Matrix;
            DrawMatrix.InverseMatrix = Parent.DrawMatrix.InverseMatrix;
        }

        DrawMatrix.Translate(X, Y, Z);

        DrawMatrix.Scale(Width * Scale.X, Height * Scale.Y, Length * Scale.Z);
        DrawMatrix.Rotate(RawRotation);
    }

    protected void InvalidateIfCamera(Invalidation invalidation)
    {
        if (this is not Camera) return;
        Invalidate(invalidation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void invalidateChildrenIfComposite(Invalidation invalidation)
    {
        if (this is CompositeDrawable3D composite) composite.InvalidateChildren(invalidation);
    }

    #region Input

    protected internal virtual bool OnMouseDown(ICursorInputEvent e)
    {
        return false;
    }

    protected internal virtual void OnMouseUp(ICursorInputEvent e)
    {
    }

    protected internal virtual bool OnKeyDown(KeyboardInputEvent e)
    {
        return false;
    }

    protected internal virtual void OnKeyUp(KeyboardInputEvent e)
    {
    }

    protected internal virtual bool OnPlatformBindingDown(PlatformActionBinding e)
    {
        return false;
    }

    protected internal virtual void OnPlatformBindingUp(PlatformActionBinding e)
    {
    }


    protected internal virtual bool OnMouseWheel(float delta)
    {
        return false;
    }

    #endregion
}
