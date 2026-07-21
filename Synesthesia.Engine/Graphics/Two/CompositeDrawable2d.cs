// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Util;
using System.Runtime.InteropServices;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.ActionBindings;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util.Pooling;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two;

public class CompositeDrawable2D : Drawable2D
{
    private readonly Lock childrenLock = new();

    [SuppressMessage("Design", "MA0016:Prefer using collection abstraction instead of implementation")]
    protected internal List<Drawable2D> InternalChildren = [];

    [Singleton]
    private OpenGlRenderer renderer = null!;

    public float CornerRadius
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.DrawNode | Invalidation.Geometry);
        }
    } = 0f;

    public bool Masking
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            Invalidate(Invalidation.All);
        }
    } = false;

    public Vector4 AutoSizePadding
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Size | Invalidation.Geometry | Invalidation.Layout);
        }
    } = Vector4.Zero;

    #region Children Management

    protected internal void InvalidateChildren(Invalidation flags)
    {
        using var snapshot = Snapshot.Rent(InternalChildren);

        foreach (ref Drawable2D internalChild in snapshot.Span)
        {
            internalChild.Invalidate(flags);
        }
    }

    public IEnumerable<Drawable2D> Children
    {
        get => InternalChildren;
        set
        {
            lock (childrenLock)
            {
                if (InternalChildren.Count > 0)
                {
                    foreach (ref Drawable2D oldChild in CollectionsMarshal.AsSpan(InternalChildren))
                    {
                        oldChild.Parent = null;
                        oldChild.Dispose();
                    }

                    InternalChildren.Clear();
                }

                InternalChildren = value.ToList();
                foreach (var child in value)
                {
                    child.Parent = this;
                    if (IsLoaded)
                    {
                        child.Load();
                    }
                }

                Invalidate(Invalidation.All);
            }
        }
    }

    public void AddChild(Drawable2D child)
    {
        lock (childrenLock)
        {
            InternalChildren.Add(child);
            child.Parent = this;
            child.Load();
            Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    }

    public void RemoveChild(Drawable2D child)
    {
        lock (childrenLock)
        {
            InternalChildren.Remove(child);
            child.Dispose();
            Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    }

    #endregion

    #region Lifecycle

    protected override void InternalLoadComplete()
    {
        lock (childrenLock)
        {
            foreach (ref Drawable2D internalChild in CollectionsMarshal.AsSpan(InternalChildren))
            {
                internalChild.Load();
            }
        }

        UpdateLayout();

        base.InternalLoadComplete();
    }


    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if (!Visible) return;

        Snapshot<Drawable2D> snapshot;
        lock (childrenLock)
        {
            snapshot = Snapshot.Rent(InternalChildren);
        }

        using (snapshot)
        {
            foreach (ref Drawable2D child in snapshot.Span)
            {
                child.OnUpdate(frameInfo);
            }
        }

        base.OnUpdate(frameInfo);
    }

    protected override void OnDraw2d()
    {
        Snapshot<Drawable2D> snapshot;

        lock (childrenLock)
        {
            snapshot = Snapshot.Rent(InternalChildren);
        }

        if (Masking)
        {
            renderer.VertexBatch2D.Flush();

            renderer.BeginStencil();
            renderer.BeginStencilMask();

            drawStencilQuad();

            renderer.VertexBatch2D.Flush();
            renderer.EndStencilMask();
        }

        using (snapshot)
        {
            foreach (ref Drawable2D child in snapshot.Span)
            {
                child.OnDraw();
            }
        }

        if (Masking)
        {
            renderer.VertexBatch2D.Flush();

            renderer.BeginStencilRestore();
            drawStencilQuad();
            renderer.VertexBatch2D.Flush();

            renderer.EndStencil();
        }

        if (BorderThickness > 0)
        {
            renderer.DrawQuad(
                drawMatrix: DrawMatrix,
                position: Vector2.Zero,
                size: Size,
                packedColor: Color.TransparentPacked,
                alpha: InheritedAlpha,
                borderThickness: BorderThickness,
                borderHasSingleColor: BorderColor.HasSingleColor,
                borderColor: CachedBorderColor,
                cornerRadius: CornerRadius
            );
        }
    }

    private void drawStencilQuad()
    {
        renderer.DrawQuad(
            drawMatrix: DrawMatrix,
            position: Vector2.Zero,
            size: Size,
            packedColor: Color.WhitePacked,
            alpha: InheritedAlpha,
            borderThickness: BorderThickness,
            borderHasSingleColor: true,
            borderColor: CachedBorderColor,
            cornerRadius: CornerRadius
        );
    }

    #endregion

    #region Layout

    protected override void OnLayout(Invalidation dirty)
    {
        base.OnLayout(dirty);

        if (dirty.HasFlagFast(Invalidation.Size) && AutoSizeAxes != Axes.None)
        {
            UpdateAutoSize();
        }
    }

    protected virtual void UpdateAutoSize()
    {
        var childrenSize = GetChildrenSize();

        if (AutoSizeAxes.HasFlagFast(Axes.X)) Width = childrenSize.X + AutoSizePadding.X + AutoSizePadding.Z;
        if (AutoSizeAxes.HasFlagFast(Axes.Y)) Height = childrenSize.Y + AutoSizePadding.Y + AutoSizePadding.W;
    }

    public Vector2 GetChildrenSize()
    {
        lock (childrenLock)
        {
            if (InternalChildren.Count == 0) return Vector2.Zero;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                var scaledSize = child.Size * child.Scale;

                minX = Math.Min(minX, child.Position.X);
                minY = Math.Min(minY, child.Position.Y);
                maxX = Math.Max(maxX, child.Position.X + scaledSize.X);
                maxY = Math.Max(maxY, child.Position.Y + scaledSize.Y);
            }

            return new Vector2(maxX - minX, maxY - minY);
        }
    }

    public IList<Drawable2D> GetFlattenedChildrenList()
    {
        var list = new List<Drawable2D>();
        getChildrenRecursive(this, list);
        return list;
    }

    private static void getChildrenRecursive(CompositeDrawable2D compositeDrawable2D, List<Drawable2D> outList)
    {
        foreach (ref Drawable2D child in CollectionsMarshal.AsSpan(compositeDrawable2D.InternalChildren))
        {
            outList.Add(child);
            if (child is CompositeDrawable2D compositeChild)
            {
                getChildrenRecursive(compositeChild, outList);
            }
        }
    }

    #endregion

    #region Input

    protected internal void UpdateHoverState(IPositionalInputEvent e)
    {
        var handled = false;
        lock (childrenLock)
        {
            for (var i = InternalChildren.Count - 1; i >= 0; i--)
            {
                var child = InternalChildren[i];
                if (!child.CanHandleInput) continue;

                var containsMouse = child.Contains(e.Position);

                if (handled | !containsMouse)
                {
                    if (child.IsHovered)
                    {
                        child.IsHovered = false;
                        child.OnHoverLost(e);
                    }
                }
                else
                {
                    if (!child.IsHovered)
                    {
                        if (child.OnHover(e))
                        {
                            child.IsHovered = true;
                            handled = true;
                        }
                    }
                    else
                    {
                        handled = true;
                    }
                }

                if (child is CompositeDrawable2D composite)
                {
                    composite.UpdateHoverState(e);
                }
            }
        }
    }

    protected internal void UpdateKeyState(KeyboardInputEvent keyboardInputEvent)
    {
        var down = keyboardInputEvent.IsDown;
        for (int i = InternalChildren.Count - 1; i >= 0; i--)
        {
            var child = InternalChildren[i];
            if (!child.CanHandleInput) continue;

            var handled = down && child.OnKeyDown(keyboardInputEvent);
            if (!down) child.OnKeyUp(keyboardInputEvent);

            if (handled) break;

            if (child is CompositeDrawable2D composite)
                composite.UpdateKeyState(keyboardInputEvent);
        }
    }

    protected internal void UpdatePlatformActionBindingState(PlatformActionBinding platformActionBinding)
    {
        var down = platformActionBinding.IsDown;
        for (int i = InternalChildren.Count - 1; i >= 0; i--)
        {
            var child = InternalChildren[i];
            if (!child.CanHandleInput) continue;

            var handled = down && child.OnPlatformBindingDown(platformActionBinding);
            if (!down) child.OnPlatformBindingUp(platformActionBinding);

            if (handled) break;

            if (child is CompositeDrawable2D composite)
                composite.UpdatePlatformActionBindingState(platformActionBinding);
        }
    }


    protected internal void UpdateCursorInputState(ICursorInputEvent e)
    {
        var down = e.IsDown;
        for (var i = InternalChildren.Count - 1; i >= 0; i--)
        {
            var child = InternalChildren[i];
            if (!child.CanHandleInput) continue;

            if (down && child is { IsMouseDown: false} && child.Contains(InputHandler.MousePosition) && child.OnMouseDown(e))
            {
                child.IsMouseDown = true;
            }

            if (!down && child.IsMouseDown)
            {
                child.IsMouseDown = false;
                child.OnMouseUp(e);
            }

            if (child is CompositeDrawable2D drawable2D)
            {
                drawable2D.UpdateCursorInputState(e);
            }
        }
    }

    protected internal void UpdateScrollWheelState(MouseScrollInputEvent e)
    {
        foreach (var child in InternalChildren.Filter(c => c.CanHandleInput && c.Contains(InputHandler.MousePosition)).Reversed())
        {
            var handled = child.OnMouseWheel(e.Delta);

            if (handled) continue;

            if (child is CompositeDrawable2D drawable2D)
            {
                drawable2D.UpdateScrollWheelState(e);
            }
        }
    }

    #endregion

    protected override void Dispose(bool isDisposing)
    {
        lock (childrenLock)
        {
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];

                if (child is IPooledObject { IsPooled: true, ReturnAction: not null } pooledObject)
                {
                    pooledObject.ReturnAction.Invoke(pooledObject);
                }
                else
                {
                    child.Dispose();
                }
            }

            InternalChildren.Clear();
        }

        base.Dispose(isDisposing);
    }
}
