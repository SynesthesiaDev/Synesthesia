// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using FreeTypeSharp;

namespace Synesthesia.Engine.Graphics.Textures;

public class FontAtlas : IDisposable
{
    private const string default_charset =
        " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
        "ěščřžýáíéůúťďňĚŠČŘŽÝÁÍÉŮÚŤĎŇ" +
        "äöüßÄÖÜñçàèìòùÀÈÌÒÙ" +
        "€$£¥©®™°±«»„“”“‘’…–—" +
        "¿¡†‡";

    public required TextureAtlas TextureAtlas { get; init; } = null!;

    public required IDictionary<char, GlyphInfo> Glyphs { get; init; } = null!;

    public required float LineHeight { get; init; }

    public static FontAtlas Generate(FreeTypeFaceFacade face, int fontSize, string charset = default_charset)
    {
        var builder = new TextureAtlasBuilder();
        var glyphMeta = new Dictionary<char, (Vector2 bearing, int advance)>();

        unsafe
        {
            FT.FT_Set_Pixel_Sizes(face.FaceRec, 0, (uint)fontSize);

            var lineHeight = face.FaceRec->size->metrics.height >> 6;

            foreach (var c in charset)
            {
                FT.FT_Load_Char(face.FaceRec, c, FT_LOAD.FT_LOAD_RENDER);
                var glyph = face.FaceRec->glyph;
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

            return new FontAtlas { TextureAtlas = textureAtlas, LineHeight = lineHeight, Glyphs = glyphInfoMap };
        }
    }

    public void Dispose()
    {
        TextureAtlas.Dispose();
        Glyphs.Clear();
    }
}
