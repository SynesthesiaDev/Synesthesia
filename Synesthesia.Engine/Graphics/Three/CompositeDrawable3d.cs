namespace Synesthesia.Engine.Graphics.Three;

public class CompositeDrawable3d : Drawable3d
{
    private List<Drawable3d> internalChildren = [];
    public List<Drawable3d> Children
    {
        get => internalChildren;
        set
        {
            internalChildren = value;
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
