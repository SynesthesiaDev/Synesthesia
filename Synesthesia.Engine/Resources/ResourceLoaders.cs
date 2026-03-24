// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Silk.NET.OpenGL;
using StbImageSharp;
using Texture = Synesthesia.Engine.Graphics.Texture;

namespace Synesthesia.Engine.Resources;

public static class ResourceLoaders
{
    public static Texture LoadTexture(Stream stream, bool uploadImmediately = false)
    {
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        return new Texture(image.Width, image.Height, image.Data, PixelFormat.Rgba, uploadImmediately);
    }
}
