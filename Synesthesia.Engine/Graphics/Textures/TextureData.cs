// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Binary;
using Silk.NET.OpenGL;
using Buffer = System.Buffer;

namespace Synesthesia.Engine.Graphics.Textures;

public class TextureData(int width, int height, byte[] data, PixelFormat pixelFormat)
{
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;
    public byte[] Data { get; set; } = data;
    public PixelFormat PixelFormat { get; set; } = pixelFormat;

    public static readonly IBinaryCodec<TextureData> BINARY_CODEC = BinaryCodec.Of<int, int, byte[], PixelFormat, TextureData>(
        BinaryCodec.INT, d => d.Width,
        BinaryCodec.INT, d => d.Height,
        BinaryCodec.BYTE_ARRAY, d => d.Data,
        BinaryCodec.Enum<PixelFormat>(), d => d.PixelFormat,
        (w, h, data, format) =>
        {
            var newArray = new byte[data.Length];
            Buffer.BlockCopy(data, 0, newArray, 0, data.Length);
            return new TextureData(w, h, newArray, format);
        });

    public override string ToString() => $"TextureData(Width={Width}, Height={Height}, Data={Data.Length} PixelFormat={PixelFormat})";
}
