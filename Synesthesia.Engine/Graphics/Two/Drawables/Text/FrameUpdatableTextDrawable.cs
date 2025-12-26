namespace Synesthesia.Engine.Graphics.Two.Drawables.Text;

public class FrameUpdatableTextDrawable : TextDrawable
{
    public Func<string>? UpdateOnDraw { get; set; } = null;

    protected internal override void OnUpdate()
    {
        if (UpdateOnDraw == null) return;
        Text = UpdateOnDraw!.Invoke();
    }
}