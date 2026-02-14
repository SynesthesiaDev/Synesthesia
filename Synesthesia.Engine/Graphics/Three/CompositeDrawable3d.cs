using System.Buffers;
using System.Numerics;
using Common.Util;

namespace Synesthesia.Engine.Graphics.Three;

public class CompositeDrawable3d : Drawable3d
{
    protected List<Drawable3d> InternalChildren = [];

    private readonly object childrenLock = new();

    public IEnumerable<Drawable3d> Children
    {
        get => InternalChildren;
        set
        {
            lock (childrenLock)
            {
                if (InternalChildren.Count > 0)
                {
                    foreach (var oldChild in InternalChildren)
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
                    child.Load();
                }
            }
        }
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        Snapshot<Drawable3d> snapshot;
        lock (childrenLock)
        {
            snapshot = Snapshot.Rent(InternalChildren);
        }

        try
        {
            for (int i = 0; i < snapshot.Count; i++)
            {
                snapshot.Array[i].OnUpdate(frameInfo);
            }
        }
        finally
        {
            snapshot.Return();
        }

        base.OnUpdate(frameInfo);
    }

    protected override void OnDraw3d()
    {
        Drawable3d[] snapshot;
        int count = 0;

        lock (childrenLock)
        {
            snapshot = ArrayPool<Drawable3d>.Shared.Rent(InternalChildren.Count);
            foreach (var child in InternalChildren.Where(child => child.Visible))
            {
                snapshot[count++] = child;
            }
        }

        try
        {
            for (int i = 0; i < count; i++)
            {
                snapshot[i].OnDraw();
            }
        }
        finally
        {
            ArrayPool<Drawable3d>.Shared.Return(snapshot, true);
        }
    }

    public void AddChild(Drawable3d child)
    {
        lock (childrenLock)
        {
            InternalChildren.Add(child);
            child.Parent = this;
            child.Load();
        }
    }

    public void RemoveChild(Drawable3d child)
    {
        lock (childrenLock)
        {
            InternalChildren.Remove(child);
            child.Dispose();
        }
    }


    protected override void Dispose(bool isDisposing)
    {
        lock (childrenLock)
        {
            InternalChildren.ForEach(c => c.Dispose());
            InternalChildren.Clear();
        }

        base.Dispose(isDisposing);
    }

    public Vector3 GetChildrenSize()
    {
        if (InternalChildren.Count == 0) return Vector3.Zero;

        float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

        foreach (var child in InternalChildren.ToArray())
        {
            var scaledSize = child.Size * child.Scale;

            minX = Math.Min(minX, child.Position.X);
            minY = Math.Min(minY, child.Position.Y);
            minZ = Math.Min(minZ, child.Position.Z);

            maxX = Math.Max(maxX, child.Position.X + scaledSize.X);
            maxY = Math.Max(maxY, child.Position.Y + scaledSize.Y);
            maxZ = Math.Max(maxZ, child.Position.Z + scaledSize.Z);
        }

        return new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
    }

    public List<Drawable3d> GetFlattenedChildrenList()
    {
        var list = new List<Drawable3d>();
        getChildrenRecursive(this, list);
        return list;
    }

    private static void getChildrenRecursive(CompositeDrawable3d compositeDrawable3d, List<Drawable3d> outList)
    {
        foreach (var child in compositeDrawable3d.InternalChildren)
        {
            outList.Add(child);
            if (child is CompositeDrawable3d compositeChild)
            {
                getChildrenRecursive(compositeChild, outList);
            }
        }
    }

}
