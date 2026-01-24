using System.Numerics;
using Raylib_cs;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Threading.Runners;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Text;

public class TextDrawable : ColoredDrawable2d
{
    public static readonly Font DEFAULT_FONT = ResourceManager.Get<Font>("SynesthesiaResources.Quicksand-Regular.ttf");

    private Font font = DEFAULT_FONT;
    private string text = string.Empty;
    private float fontSize = 24f;
    private float spacing = 0f;

    public Font Font
    {
        get => font;
        set
        {
            font = value;
            updateSize();
        }
    }

    public string Text
    {
        get => text;
        set
        {
            text = value;
            updateSize();
        }
    }

    public float FontSize
    {
        get => fontSize;
        set
        {
            fontSize = value;
            updateSize();
        }
    }

    public float Spacing
    {
        get => spacing;
        set
        {
            spacing = value;
            updateSize();
        }
    }

    protected override void OnDraw2d()
    {
        var shader = RenderThreadRunner.SignedDistanceFieldShader;
        var sizeLoc = Raylib.GetShaderLocation(shader, "renderSize");
        var alphaLoc = Raylib.GetShaderLocation(shader, "alpha");
        
        Raylib.SetShaderValue(shader, sizeLoc, FontSize, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(shader, alphaLoc, InheritedAlpha, ShaderUniformDataType.Float);
        
        Raylib.BeginShaderMode(shader);
        Raylib.DrawTextEx(Font, Text, Vector2.Zero, FontSize, Spacing, ApplyAlpha(Color));
        Raylib.EndShaderMode();
    }

    private void updateSize()
    {
        Size = Raylib.MeasureTextEx(Font, Text, FontSize, Spacing);
    }
}