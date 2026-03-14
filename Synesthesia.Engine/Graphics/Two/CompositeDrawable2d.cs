// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Util;
using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Graphics.Two;

public class CompositeDrawable2d : Drawable2d
{
    private readonly Lock childrenLock = new();

    [SuppressMessage("Design", "MA0016:Prefer using collection abstraction instead of implementation")]
    protected internal List<Drawable2d> InternalChildren = [];

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


    protected override void OnDraw2d()
    {
    }
}
