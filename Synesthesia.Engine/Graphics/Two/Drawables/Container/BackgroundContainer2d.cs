using Raylib_cs;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
namespace Synesthesia.Engine.Graphics.Two.Drawables.Container;

public class BackgroundContainer2d : Container2d
{
    public Color? BackgroundColor { get; set; } = null;

    public float BackgroundAlpha { get; set; } = 1f;

    public float BackgroundCornerRadius { get; set; } = 0f;

    private readonly DrawableBox2d _background = new();

    protected override void OnDraw2d()
    {
        DrawBackground();
        base.OnDraw2d();
    }

    protected void DrawBackground()
    {
        if (BackgroundColor == null) return;
        _background.Size = Size;
        _background.Parent = this;

        _background.CornerRadius = BackgroundCornerRadius;
        _background.Color = (Color)BackgroundColor;
        _background.Alpha = BackgroundAlpha;

        _background.OnDraw();
    }
}