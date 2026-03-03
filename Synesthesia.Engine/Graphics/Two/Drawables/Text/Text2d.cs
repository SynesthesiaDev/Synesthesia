using System.Numerics;
using Raylib_cs;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Fonts;
using Synesthesia.Engine.Graphics.Shaders;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Threading.Runners;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Text;

public class Text2d : ColoredDrawable2d
{
    [Resolved]
    private IResourceStore<Font> fontResourceStore = null!;

    [Resolved]
    private IResourceStore<Shader> shaderResourceStore = null!;

    private Shader? sdfShader;

    [Resolved]
    private RenderThread renderThread = null!;

    private Font? defaultFont;

    public Font? Font
    {
        get;
        set
        {
            field = value;
            updateSize();
        }
    }

    public string Text
    {
        get;
        set
        {
            field = value;
            updateSize();
        }
    } = string.Empty;

    public float FontSize
    {
        get;
        set
        {
            field = value;
            updateSize();
        }
    } = 24f;

    public float Spacing
    {
        get;
        set
        {
            field = value;
            updateSize();
        }
    }

    protected override void OnLoading()
    {
        renderThread.Schedule(() =>
        {
            defaultFont = fontResourceStore.Get("Synesthesia.Resources.Quicksand-Regular.ttf");
            Font = defaultFont;
        });
    }

    protected override void OnDraw2d()
    {
        if(Font == null) return;

        sdfShader ??= shaderResourceStore.Get("Synesthesia.Resources.Shaders.sdf_font.fsh");

        var sizeLoc = Raylib.GetShaderLocation(sdfShader.NativeShader, "renderSize");
        var alphaLoc = Raylib.GetShaderLocation(sdfShader.NativeShader, "alpha");

        Raylib.SetShaderValue(sdfShader.NativeShader, sizeLoc, FontSize, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(sdfShader.NativeShader, alphaLoc, InheritedAlpha, ShaderUniformDataType.Float);

        Raylib.BeginShaderMode(sdfShader.NativeShader);

        Raylib.DrawTextEx(Font.NativeFont, Text, Vector2.Zero, FontSize, Spacing, ApplyAlpha(Color));
        Raylib.EndShaderMode();
    }

    private void updateSize()
    {
        if (Font == null) return;
        Size = Raylib.MeasureTextEx(Font.NativeFont, Text, FontSize, Spacing);
    }
}
