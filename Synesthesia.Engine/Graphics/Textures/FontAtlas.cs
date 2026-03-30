// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using FreeTypeSharp;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Graphics.Textures;

public class FontAtlas : IDisposable
{
    private const string default_charset =
        " !?\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
        "ěščřžýáíéůúťďňĚŠČŘŽÝÁÍÉŮÚŤĎŇ" +
        "äöüßÄÖÜñçàèìòùÀÈÌÒÙ" +
        "€$£¥©®™°±«»„“”“‘’…–—" +
        "¿¡†‡§ ";

    public const int RENDER_SIZE = 32;

    public required TextureAtlas TextureAtlas { get; init; } = null!;

    public required IDictionary<char, GlyphInfo> Glyphs { get; init; } = null!;

    public required float LineHeight { get; init; }
    public required float Ascent { get; init; }
    public required float Descent { get; init; }

    public FontAtlas() => EngineStatistics.Increment(EngineStatistics.Type.FontAtlases);

    public static FontAtlas Generate(FreeTypeFaceFacade face, string charset = default_charset)
    {
        var builder = new TextureAtlasBuilder();
        var glyphMeta = new Dictionary<char, (Vector2 bearing, int advance)>();
        builder.SetPadding(8);

        unsafe
        {
            FT.FT_Set_Pixel_Sizes(face.FaceRec, 0, RENDER_SIZE);

            var metrics = face.FaceRec->size->metrics;
            var ascent = metrics.ascender >> 6;
            var descent = metrics.descender >> 6;
            var lineHeight = metrics.height >> 6;

            foreach (var c in charset.Distinct())
            {
                FT.FT_Load_Char(face.FaceRec, c, FT_LOAD.FT_LOAD_RENDER);

                var glyph = face.FaceRec->glyph;
                var advance = glyph->advance.x.ToInt32() >> 6;
                var bearing = new Vector2(glyph->bitmap_left, glyph->bitmap_top);

                if (glyph->bitmap.width == 0 || glyph->bitmap.rows == 0)
                {
                    glyphMeta[c] = (bearing, advance);
                    continue;
                }

                FT.FT_Render_Glyph(glyph, FT_Render_Mode_.FT_RENDER_MODE_SDF);

                var bitmap = glyph->bitmap;

                if (bitmap.width == 0 || bitmap.rows == 0) continue;

                var rgba = new byte[bitmap.width * bitmap.rows * 4];
                for (int i = 0; i < (int)(bitmap.width * bitmap.rows); i++)
                {
                    rgba[i * 4 + 0] = 255;
                    rgba[i * 4 + 1] = 255;
                    rgba[i * 4 + 2] = 255;
                    rgba[i * 4 + 3] = bitmap.buffer[i];
                }

                builder.Add(c, (int)bitmap.width, (int)bitmap.rows, rgba);
                glyphMeta[c] = (new Vector2(glyph->bitmap_left, glyph->bitmap_top), glyph->advance.x.ToInt32() >> 6);
            }

            var textureAtlas = builder.Build();

            var glyphInfoMap = new Dictionary<char, GlyphInfo>();
            foreach (var (c, meta) in glyphMeta)
            {
                glyphInfoMap[c] = new GlyphInfo(c, meta.bearing, meta.advance);
            }

            return new FontAtlas
            {
                TextureAtlas = textureAtlas,
                LineHeight = lineHeight,
                Glyphs = glyphInfoMap,
                Ascent = ascent,
                Descent = descent
            };
        }
    }

    public void Dispose()
    {
        TextureAtlas.Dispose();
        Glyphs.Clear();
        EngineStatistics.Decrement(EngineStatistics.Type.FontAtlases);
    }
}
