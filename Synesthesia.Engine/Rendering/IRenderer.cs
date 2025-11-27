namespace Synesthesia.Engine.Rendering;

public interface IRenderer
{
    protected internal bool VerticalSync { get; set; }

    protected internal bool AllowTearing { get; set; }
}
