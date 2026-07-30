// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;
using Codon.Binary;
using Synesthesia.Engine.Util.Codecs;

namespace Synesthesia.Engine.Graphics.Textures;

[StructLayout(LayoutKind.Auto)]
public readonly struct GlyphInfo(int regionHandle, Vector2 bearing, float advance)
{
    public readonly int RegionHandle = regionHandle;
    public readonly Vector2 Bearing = bearing;
    public readonly float Advance = advance;

    public static readonly IBinaryCodec<GlyphInfo> BINARY_CODEC = BinaryCodecs.For<GlyphInfo>()
        .Field(BinaryCodecs.INT, g => g.RegionHandle)
        .Field(ExtraCodecs.VECTOR_2, g => g.Bearing)
        .Field(BinaryCodecs.FLOAT, g => g.Advance)
        .Build((handle, bearing, advance) => new GlyphInfo(handle, bearing, advance));
}
