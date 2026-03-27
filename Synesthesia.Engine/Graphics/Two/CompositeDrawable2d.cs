// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Util;
using System.Runtime.InteropServices;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util.Pooling;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two;

public class CompositeDrawable2d : Drawable2d
{
    private readonly Lock childrenLock = new();

    [SuppressMessage("Design", "MA0016:Prefer using collection abstraction instead of implementation")]
    protected internal List<Drawable2d> InternalChildren = [];

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

    public ComplexColor BorderColor
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Invalidate(Invalidation.DrawNode);
        }
    } = ComplexColor.Black;

    public int BorderThickness
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.DrawNode);
        }
    } = 0;

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

        foreach (ref Drawable2d internalChild in snapshot.Span)
        {
            internalChild.Invalidate(flags);
        }
    }

    public IEnumerable<Drawable2d> Children
    {
        get => InternalChildren;
        set
        {
            lock (childrenLock)
            {
                if (InternalChildren.Count > 0)
                {
                    foreach (ref Drawable2d oldChild in CollectionsMarshal.AsSpan(InternalChildren))
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

    public void AddChild(Drawable2d child)
    {
        lock (childrenLock)
        {
            InternalChildren.Add(child);
            child.Parent = this;
            child.Load();
            Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    }

    public void RemoveChild(Drawable2d child)
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
            foreach (ref Drawable2d internalChild in CollectionsMarshal.AsSpan(InternalChildren))
            {
                internalChild.Load();
            }
        }

        UpdateLayout();

        base.InternalLoadComplete();
    }


    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if(!Visible) return;

        Snapshot<Drawable2d> snapshot;
        lock (childrenLock)
        {
            snapshot = Snapshot.Rent(InternalChildren);
        }

        using (snapshot)
        {
            foreach (ref Drawable2d child in snapshot.Span)
            {
                child.OnUpdate(frameInfo);
            }
        }

        base.OnUpdate(frameInfo);
    }

    protected override void OnDraw2d()
    {
        Snapshot<Drawable2d> snapshot;

        lock (childrenLock)
        {
            snapshot = Snapshot.Rent(InternalChildren);
        }

        if (Masking)
        {
            renderer.BeginStencil();
            renderer.BeginStencilMask();

            // var maskRect = new Rectangle(0, 0, (int)Size.X, (int)Size.Y);
            //TODO draw rect

            renderer.EndStencilMask();
        }

        using (snapshot)
        {
            foreach (ref Drawable2d child in snapshot.Span)
            {
                child.OnDraw();
            }
        }

        if (Masking) renderer.EndStencil();
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

    public IList<Drawable2d> GetFlattenedChildrenList()
    {
        var list = new List<Drawable2d>();
        getChildrenRecursive(this, list);
        return list;
    }

    private static void getChildrenRecursive(CompositeDrawable2d compositeDrawable2d, List<Drawable2d> outList)
    {
        foreach (ref Drawable2d child in CollectionsMarshal.AsSpan(compositeDrawable2d.InternalChildren))
        {
            outList.Add(child);
            if (child is CompositeDrawable2d compositeChild)
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

                if (child is CompositeDrawable2d composite)
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

            if (child is CompositeDrawable2d composite)
                composite.UpdateKeyState(keyboardInputEvent);
        }
    }


    protected internal void UpdateCursorInputState(ICursorInputEvent e)
    {
        var down = e.IsDown;
        for (var i = InternalChildren.Count - 1; i >= 0; i--)
        {
            var child = InternalChildren[i];
            if (!child.CanHandleInput) continue;

            if (down && child is { IsMouseDown: false, IsHovered: true } && child.OnMouseDown(e))
            {
                Logger.Verbose($"{e} handled by {child.GetType().Name}", Logger.Input);
                child.IsMouseDown = true;
            }

            if (!down && child.IsMouseDown)
            {
                child.IsMouseDown = false;
                Logger.Verbose($"{e} handled by {child.GetType().Name}", Logger.Input);
                child.OnMouseUp(e);
            }

            if (child is CompositeDrawable2d drawable2d)
            {
                drawable2d.UpdateCursorInputState(e);
            }
        }
    }

    protected internal void UpdateScrollWheelState(MouseScrollInputEvent e)
    {
        foreach (var child in InternalChildren.Filter(c => c.CanHandleInput && c.Contains(InputHandler.MousePosition)).Reversed())
        {
            var handled = child.OnMouseWheel(e.Delta);

            if (handled) continue;

            if (child is CompositeDrawable2d drawable2d)
            {
                drawable2d.UpdateScrollWheelState(e);
            }
        }
    }


    #endregion

    protected override void Dispose(bool isDisposing)
    {
        lock (childrenLock)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
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
