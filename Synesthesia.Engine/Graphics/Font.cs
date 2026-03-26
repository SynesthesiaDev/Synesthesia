// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics.Textures;

namespace Synesthesia.Engine.Graphics;

public class Font(string name, int size, FontAtlas atlas) : IDisposable
{
    public readonly string Name = name;
    public readonly int Size = size;
    public readonly FontAtlas Atlas = atlas;

    public Vector2 MeasureText(string text)
    {
        float width = 0;
        float maxHeight = 0;

        foreach (char c in text)
        {
            if (!Atlas.Glyphs.TryGetValue(c, out var glyph))
                continue;

            var region = Atlas.TextureAtlas.GetRegion(glyph.RegionHandle);
            width += glyph.Advance;
            maxHeight = Math.Max(maxHeight, region.Size.Y);
        }

        return new Vector2(width, maxHeight);
    }

    public void Dispose()
    {
        Atlas.Dispose();
    }
}
