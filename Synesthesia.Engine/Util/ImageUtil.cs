// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Silk.NET.OpenGL;
using StbImageWriteSharp;
using Synesthesia.Engine.Graphics.Textures;

namespace Synesthesia.Engine.Util;

public static class ImageUtil
{
    public static void SaveTextureToPng(TextureData texture, string filename)
    {
        using Stream stream = File.OpenWrite(filename);
        ImageWriter writer = new ImageWriter();

        ColorComponents components = texture.PixelFormat == PixelFormat.Rgba
            ? ColorComponents.RedGreenBlueAlpha
            : ColorComponents.RedGreenBlue;

        writer.WritePng(texture.Data, texture.Width, texture.Height, components, stream);
        writer.WritePng(texture.Data, texture.Width, texture.Height, components, stream);
    }
}
