// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.Numerics;

namespace Synesthesia.Engine.Graphics.Textures;

public readonly struct TextureRegion
{
    public readonly Texture Texture;
    public readonly RectangleF UvRect;
    public readonly Vector2 Size;

    public TextureRegion(Texture texture)
    {
        Texture = texture;
        UvRect = new RectangleF(0, 0, 1, 1);
        Size = new Vector2(Texture.Width, Texture.Height);
    }

    public TextureRegion(Texture texture, RectangleF uvRect, Vector2 size)
    {
        Texture = texture;
        UvRect = uvRect;
        Size = size;
    }
}
