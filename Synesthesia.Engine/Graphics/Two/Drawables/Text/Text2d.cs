using System.Numerics;
using Raylib_cs;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Font;
using Synesthesia.Engine.Graphics.Shader;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Threading.Runners;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Text;

public class Text2d : ColoredDrawable2d
{
    [Resolved]
    private IResourceStore<FontHandle> fontResourceStore = null!;

    [Resolved]
    private IResourceStore<ShaderHandle> shaderResourceStore = null!;

    private ShaderHandle? sdfShader;

    [Resolved]
    private RenderThread renderThread = null!;

    private FontHandle? defaultFont;

    public FontHandle? Font
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
