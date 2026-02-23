namespace Synesthesia.Engine.Graphics.Two.Drawables.Text;


[Obsolete("GC Hell, do not use")]
public class FrameUpdatableText2d : Text2d
{
    public Func<string>? UpdateOnDraw { get; set; } = null;

    private string last = string.Empty;

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if (UpdateOnDraw == null) return;
        var newString = UpdateOnDraw.Invoke();
        if(newString == last) return;
        last = newString;

        Text = UpdateOnDraw!.Invoke();
    }
}
