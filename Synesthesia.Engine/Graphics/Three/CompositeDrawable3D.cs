// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util.Pooling;

namespace Synesthesia.Engine.Graphics.Three;

// ReSharper disable InconsistentlySynchronizedField
public class CompositeDrawable3D : Drawable3D
{
    protected readonly Lock ChildrenLock = new();

    [SuppressMessage("Design", "MA0016:Prefer using collection abstraction instead of implementation")]
    protected internal List<Drawable3D> InternalChildren = [];

    #region Children Management

    protected internal void InvalidateChildren(Invalidation flags)
    {
        using var snapshot = Snapshot.Rent(InternalChildren);

        foreach (ref Drawable3D internalChild in snapshot.Span)
        {
            internalChild.Invalidate(flags);
        }
    }

    public IEnumerable<Drawable3D> Children
    {
        get => InternalChildren;
        set
        {
            lock (ChildrenLock)
            {
                if (InternalChildren.Count > 0)
                {
                    foreach (ref Drawable3D oldChild in CollectionsMarshal.AsSpan(InternalChildren))
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

    public void AddChild(Drawable3D child)
    {
        lock (ChildrenLock)
        {
            InternalChildren.Add(child);
            child.Parent = this;
            child.Load();
            Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    }

    public void RemoveChild(Drawable3D child)
    {
        lock (ChildrenLock)
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
        lock (ChildrenLock)
        {
            foreach (ref Drawable3D internalChild in CollectionsMarshal.AsSpan(InternalChildren))
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

        Snapshot<Drawable3D> snapshot;
        lock (ChildrenLock)
        {
            snapshot = Snapshot.Rent(InternalChildren);
        }

        using (snapshot)
        {
            foreach (ref Drawable3D child in snapshot.Span)
            {
                child.OnUpdate(frameInfo);
            }
        }

        base.OnUpdate(frameInfo);
    }

    protected override void OnDraw3D()
    {
        Snapshot<Drawable3D> snapshot;

        lock (ChildrenLock)
        {
            snapshot = Snapshot.Rent(InternalChildren);
        }

        using (snapshot)
        {
            foreach (ref Drawable3D child in snapshot.Span)
            {
                child.OnDraw();
            }
        }
    }

    #endregion

    protected override void Dispose(bool isDisposing)
    {
        lock (ChildrenLock)
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

    public IList<Drawable3D> GetFlattenedChildrenList()
    {
        var list = new List<Drawable3D>();
        getChildrenRecursive(this, list);
        return list;
    }

    private static void getChildrenRecursive(CompositeDrawable3D compositeDrawable2D, List<Drawable3D> outList)
    {
        foreach (ref Drawable3D child in CollectionsMarshal.AsSpan(compositeDrawable2D.InternalChildren))
        {
            outList.Add(child);
            if (child is CompositeDrawable3D compositeChild)
            {
                getChildrenRecursive(compositeChild, outList);
            }
        }
    }

}
