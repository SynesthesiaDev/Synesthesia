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
    [Singleton]
    private OpenGlRenderer renderer = null!;

    [Singleton]
    private IResourceStore<Font> fontResourceStore = null!;

    private Font defaultFont = null!;

    public string Text
    {
        get;
        set
        {
            if(string.Equals(field, value, StringComparison.Ordinal)) return;
            field = value;
            Invalidate(Invalidation.Size);
        }
    } = "";

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
            Size = Font.MeasureText(Text);
        }
    }

    protected override void OnDraw2d()
    {
        if (!Font.Atlas.TextureAtlas.IsUploaded || string.IsNullOrWhiteSpace(Text)) return;

        var cursorX = 0f;
        var baselineY = Font.Atlas.LineHeight;

        foreach (var character in Text)
        {
            var c = character;
            if (!Font.Atlas.Glyphs.TryGetValue(c, out var glyph))
            {
                if (c == ' ')
                {
                    cursorX += Font.Size / 3f;
                    continue;
                }

                c = '?';
                glyph = Font.Atlas.Glyphs[c];
            }

            var region = Font.Atlas.TextureAtlas.GetRegionOrNull(glyph.RegionHandle);
            if (region == null) throw new InvalidOperationException($"Failed to get texture region for font atlas with handle {glyph.RegionHandle} (character '{c}')");

            var charPos = new Vector2(cursorX + glyph.Bearing.X, baselineY - glyph.Bearing.Y);

            renderer.DrawQuad(
                DrawMatrix,
                charPos,
                region.Value.Size,
                Color.ToRgba32(),
                texture: region.Value.Texture,
                textureCoord: region.Value.UvRect
            );

            cursorX += glyph.Advance;
        }
    }
}
