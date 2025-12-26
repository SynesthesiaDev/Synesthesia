namespace Synesthesia.Engine.Graphics.Three;

public class CompositeDrawable3d : Drawable3d
{
    public List<Drawable3d> _children = [];
    public List<Drawable3d> Children
    {
        get => _children;
        set
        {
            _children = value;
            foreach (var child in value)
            {
                child.Load();
            }
        }
    }

    protected override void OnDraw3d()
    {
        foreach (var child in Children)
        {
            child.Parent = this;
            child.OnDraw();
        }
    }

    protected override void Dispose(bool isDisposing)
    {
        Children.ForEach(c => c.Dispose());
        base.Dispose(isDisposing);
    }
}