// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Codon.Binary;
using Synesthesia.Engine.Util.Codecs;

namespace Synesthesia.Engine.Graphics.Textures;

[StructLayout(LayoutKind.Auto)]
public readonly struct TextureRegion(RectangleF uvRect, Vector2 size)
{
    public readonly RectangleF UvRect = uvRect;
    public readonly Vector2 Size = size;

    public static readonly IBinaryCodec<TextureRegion> BINARY_CODEC = BinaryCodec.Of
    (
        ExtraCodecs.RECTANGLE_F, r => r.UvRect,
        ExtraCodecs.VECTOR_2, r => r.Size,
        (uv, size) => new TextureRegion(uv, size)
    );
}
