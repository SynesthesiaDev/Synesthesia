// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Binary;
using Silk.NET.OpenGL;

namespace Synesthesia.Engine.Graphics.Textures;

public class TextureData(int width, int height, byte[] data, PixelFormat pixelFormat)
{
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;
    public byte[] Data { get; set; } = data;
    public PixelFormat PixelFormat { get; set; } = pixelFormat;

    public static readonly IBinaryCodec<TextureData> BINARY_CODEC = BinaryCodecs.For<TextureData>()
        .Field(BinaryCodecs.INT, d => d.Width)
        .Field(BinaryCodecs.INT, d => d.Height)
        .Field(BinaryCodecs.BYTE_ARRAY, d => d.Data)
        .Field(BinaryCodecs.Enum<PixelFormat>(), d => d.PixelFormat)
        .Build((w, h, data, format) => new TextureData(w, h, data, format));

    public override string ToString() => $"TextureData(Width={Width}, Height={Height}, Data={Data.Length} PixelFormat={PixelFormat})";
}
