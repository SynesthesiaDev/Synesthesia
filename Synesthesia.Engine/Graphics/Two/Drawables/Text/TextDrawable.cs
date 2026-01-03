using System.Numerics;
using Raylib_cs;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Threading.Runners;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Text;

public class TextDrawable : ColoredDrawable2d
{
    public static readonly Font DefaultFont = ResourceManager.Get<Font>("SynesthesiaResources.Quicksand-Regular.ttf");

    private Font _font = DefaultFont;
    private string _text = string.Empty;
    private float _fontSize = 24f;
    private float _spacing = 0f;

    public Font Font
    {
        get => _font;
        set
        {
            _font = value;
            updateSize();
        }
    }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            updateSize();
        }
    }

    public float FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value;
            updateSize();
        }
    }

    public float Spacing
    {
        get => _spacing;
        set
        {
            _spacing = value;
            updateSize();
        }
    }

    protected override void OnDraw2d()
    {
        var renderSizeLoc = Raylib.GetShaderLocation(RenderThreadRunner.SignedDistanceFieldShader, "renderSize");
        Raylib.SetShaderValue(RenderThreadRunner.SignedDistanceFieldShader, renderSizeLoc, FontSize > 0 ? FontSize : 1.0f, ShaderUniformDataType.Float);

        Raylib.BeginShaderMode(RenderThreadRunner.SignedDistanceFieldShader);
        Raylib.DrawTextEx(Font, Text, Vector2.Zero, FontSize, Spacing, applyAlpha(Color));
        Raylib.EndShaderMode();
    }

    private void updateSize()
    {
        Size = Raylib.MeasureTextEx(Font, Text, FontSize, Spacing);
    }
}