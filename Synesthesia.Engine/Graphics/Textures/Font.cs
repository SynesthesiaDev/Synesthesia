// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Codon.Binary;

namespace Synesthesia.Engine.Graphics.Textures;

public class Font(int size, FontAtlas atlas) : IDisposable
{
    public readonly int Size = size;
    public readonly FontAtlas Atlas = atlas;

    public static readonly IBinaryCodec<Font> BINARY_CODEC = BinaryCodec.Of
    (
        BinaryCodec.INT, f => f.Size,
        FontAtlas.BINARY_CODEC, f => f.Atlas,
        (size, atlas) => new Font(size, atlas)
    );

    public override string ToString() => $"Font(Size={Size}, Atlas={Atlas})";

    public Vector2 MeasureText(string text, float targetSize)
    {
        if (string.IsNullOrEmpty(text)) return Vector2.Zero;

        float scale = targetSize / FontAtlas.RENDER_SIZE;
        float width = 0;

        foreach (char c in text)
        {
            if (Atlas.Glyphs.TryGetValue(c, out var glyph))
            {
                width += glyph.Advance * scale;
            }
        }

        return new Vector2(width + (2 * scale), (Atlas.Ascent - Atlas.Descent) * scale);
    }

    public void Dispose()
    {
        Atlas.Dispose();
    }
}
