// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;

namespace Synesthesia.Engine.Graphics.Textures;

public class Font(string name, int size, FontAtlas atlas) : IDisposable
{
    public readonly string Name = name;
    public readonly int Size = size;
    public readonly FontAtlas Atlas = atlas;

    public Vector2 MeasureText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Vector2.Zero;

        float width = 0;

        float height = Atlas.LineHeight;

        foreach (char c in text)
        {
            if (!Atlas.Glyphs.TryGetValue(c, out var glyph))
                continue;

            width += glyph.Advance;
        }

        return new Vector2(width, height);
    }

    public void Dispose()
    {
        Atlas.Dispose();
    }
}
