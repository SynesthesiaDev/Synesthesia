using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Input;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two.Drawables;

public class CompositeDrawable2d : Drawable2d
{
    protected List<Drawable2d> InternalChildren = [];

    public Vector4 AutoSizePadding { get; set; } = new(0);

    public IEnumerable<Drawable2d> Children
    {
        get => InternalChildren;
        set
        {
            InternalChildren = value.ToList();
            foreach (var child in value)
            {
                child.Parent = this;
                child.Load();
            }
        }
    }

    protected internal void UpdateHoverState(HoverEvent e)
    {
        foreach (var child in InternalChildren.Filter(c => c.AcceptsInputs()).Reversed())
        {
            if (child.IsHovered && !child.Contains(e.MousePosition))
            {
                child.IsHovered = false;
                child.OnHoverLost(e);
            }

            if (!child.IsHovered && child.Contains(e.MousePosition) && child.OnHover(e))
            {
                child.IsHovered = true;
            }

            if (child is CompositeDrawable2d drawable2d)
            {
                drawable2d.UpdateHoverState(e);
            }
        }
    }

    protected internal void UpdatePointInputState(PointInput e, bool down)
    {
        foreach (var child in InternalChildren.Filter(c => c.AcceptsInputs()).Reversed())
        {
            if (down && !child.IsMouseDown && child.IsHovered && child.OnMouseDown(e))
            {
                child.IsMouseDown = true;
            }

            if (!down && child.IsMouseDown)
            {
                child.IsMouseDown = false;
                child.OnMouseUp(e);
            }

            if (child is CompositeDrawable2d drawable2d)
            {
                drawable2d.UpdatePointInputState(e, down);
            }
        }
    }

    protected internal void UpdateActionBindingState(ActionBinding e, bool down)
    {
        foreach (var child in InternalChildren.Filter(c => c.AcceptsInputs()).Reversed())
        {
            var handled = down && child.OnActionBindingDown(e);

            if (!down) child.OnActionBindingUp(e);

            if (handled) continue;

            if (child is CompositeDrawable2d drawable2d)
            {
                drawable2d.UpdateActionBindingState(e, down);
            }
        }
    }

    protected internal void UpdateKeyState(KeyboardKey e, bool down)
    {
        foreach (var child in InternalChildren.Filter(c => c.AcceptsInputs()).Reversed())
        {
            var handled = down && child.OnKeyDown(e);

            if (!down) child.OnKeyUp(e);

            if (handled) continue;

            if (child is CompositeDrawable2d drawable2d)
            {
                drawable2d.UpdateKeyState(e, down);
            }
        }
    }

    public void AddChild(Drawable2d child)
    {
        InternalChildren.Add(child);
        child.Parent = this;
        child.Load();
    }

    public void RemoveChild(Drawable2d child)
    {
        InternalChildren.Remove(child);
        child.Dispose();
    }

    protected internal override void OnUpdate()
    {
        foreach (var child in Children)
        {
            child.OnUpdate();
        }

        if (AutoSizeAxes != Axes.None) UpdateAutoSize();

        base.OnUpdate();
    }

    protected override void OnDraw2d()
    {
        InternalChildren.Filter(c => c.Visible).ForEach(child => child.OnDraw());
    }

    protected override void Dispose(bool isDisposing)
    {
        InternalChildren.ForEach(c => c.Dispose());
        InternalChildren.Clear();
        base.Dispose(isDisposing);
    }

    protected virtual void UpdateAutoSize()
    {
        var childrenSize = GetChildrenSize();

        var newWidth = Size.X;
        var newHeight = Size.Y;

        if (AutoSizeAxes.HasFlag(Axes.X)) newWidth = childrenSize.X + AutoSizePadding.X + AutoSizePadding.Z;
        if (AutoSizeAxes.HasFlag(Axes.Y)) newHeight = childrenSize.Y + AutoSizePadding.Y + AutoSizePadding.W;

        Size = new Vector2(newWidth, newHeight);
    }


    public Vector2 GetChildrenSize()
    {
        if (InternalChildren.Count == 0) return Vector2.Zero;

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var child in InternalChildren)
        {
            minX = Math.Min(minX, child.Position.X);
            minY = Math.Min(minY, child.Position.Y);
            maxX = Math.Max(maxX, child.Position.X + child.Size.X);
            maxY = Math.Max(maxY, child.Position.Y + child.Size.Y);
        }

        return new Vector2(maxX - minX, maxY - minY);
    }

    public List<Drawable2d> GetFlattenedChildrenList()
    {
        var list = new List<Drawable2d>();
        getChildrenRecursive(this, list);
        return list;
    }

    private static void getChildrenRecursive(CompositeDrawable2d compositeDrawable2d, List<Drawable2d> outList)
    {
        foreach (var child in compositeDrawable2d.InternalChildren)
        {
            outList.Add(child);
            if (child is CompositeDrawable2d compositeChild)
            {
                getChildrenRecursive(compositeDrawable2d, outList);
            }
        }
    }
}
