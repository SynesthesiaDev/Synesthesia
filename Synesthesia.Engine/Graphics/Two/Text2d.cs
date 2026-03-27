// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Resources;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two;

public class Text2d : Drawable2d
{
    private const float spread = 8f;

    [Singleton]
    private OpenGlRenderer renderer = null!;

    [Singleton]
    private IResourceStore<Font> fontResourceStore = null!;

    private Font defaultFont = null!;

    public float FontSize { get; set; } = 16f;

    public string Text
    {
        get;
        set
        {
            if (string.Equals(field, value, StringComparison.Ordinal)) return;
            field = value;
            Invalidate(Invalidation.Size);
        }
    } = "";

    public FontWeight Weight { get; set; } = FontWeight.Normal;

    public Font Font { get; set; } = null!;

    public Color Color { get; set; } = Color.White;

    protected override void OnLoading()
    {
        defaultFont = fontResourceStore.Get("Synesthesia.Resources.Fonts.Quicksand-Regular.ttf");
        Font = defaultFont;
        base.OnLoading();
    }

    protected override void OnLayout(Invalidation dirty)
    {
        base.OnLayout(dirty);

        if (dirty.HasFlagFast(Invalidation.Size))
        {
            Size = Font.MeasureText(Text, FontSize);
        }
    }

    protected override void OnDraw2d()
    {
        if (!Font.Atlas.TextureAtlas.IsUploaded || string.IsNullOrWhiteSpace(Text)) return;

        float scale = FontSize / FontAtlas.RENDER_SIZE;
        var cursorX = 0f;
        var baselineY = Font.Atlas.LineHeight * scale;

        foreach (var character in Text)
        {
            if (!Font.Atlas.Glyphs.TryGetValue(character, out var glyph))
            {
                if (character == ' ')
                {
                    if (Font.Atlas.Glyphs.TryGetValue(' ', out var spaceGlyph))
                        cursorX += spaceGlyph.Advance * scale;
                    else
                        cursorX += (FontSize / 3f);
                    continue;
                }
            }

            var region = Font.Atlas.TextureAtlas.GetRegionOrNull(glyph.RegionHandle);
            if (region == null) throw new InvalidOperationException($"Failed to get texture region for font atlas with handle {glyph.RegionHandle} (character '{character}')");

            var drawSize = region.Value.Size * scale;

            var charPos = new Vector2(
                cursorX + (glyph.Bearing.X - spread) * scale,
                baselineY - (glyph.Bearing.Y + spread) * scale
            );

            renderer.DrawQuad(
                DrawMatrix,
                charPos,
                drawSize,
                Color.ToRgba32(),
                texture: region.Value.Texture,
                textureCoord: region.Value.UvRect,
                vertexMode: VertexMode.Font,
                radius: getWeightRadius()
            );

            cursorX += glyph.Advance * scale;
        }
    }

    public enum FontWeight
    {
        Bold,
        Normal,
        Thin
    }

    private float getWeightRadius()
    {
        return Weight switch
        {
            FontWeight.Bold => 0.265f,
            FontWeight.Normal => 0.23f,
            FontWeight.Thin => 0.15f,
            _ => 0.23f
        };
    }
}
