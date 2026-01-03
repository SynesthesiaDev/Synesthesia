using Raylib_cs;

namespace Synesthesia.Engine.Utility;

public static unsafe class Unsafe
{
    public static unsafe Font LoadFontFromMemory(byte[] fontData)
    {
        return loadSdfFont(fontData);
    }

    private static unsafe Font loadSdfFont(byte[] fontData)
    {
        var sdfFont = new Font
        {
            BaseSize = 64,
            GlyphCount = 95
        };

        fixed (byte* pointerData = fontData)
        {
            sdfFont.Glyphs = Raylib.LoadFontData(pointerData, fontData.Length, sdfFont.BaseSize, null, sdfFont.GlyphCount, FontType.Sdf);

            var atlas = Raylib.GenImageFontAtlas(sdfFont.Glyphs, &sdfFont.Recs, sdfFont.GlyphCount, sdfFont.BaseSize, 4, 1);
            sdfFont.Texture = Raylib.LoadTextureFromImage(atlas);
            Raylib.UnloadImage(atlas);
        }

        Raylib.SetTextureFilter(sdfFont.Texture, TextureFilter.Bilinear);
        return sdfFont;
    }
}