// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using FreeTypeSharp;
using Silk.NET.OpenGL;
using StbImageSharp;
using Synesthesia.Engine.Graphics.Textures;
using Texture = Synesthesia.Engine.Graphics.Textures.Texture;

namespace Synesthesia.Engine.Resources;

public static class ResourceLoaders
{
    public static Texture LoadTexture(Stream stream, string name, bool uploadImmediately = false)
    {
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        return new Texture(image.Width, image.Height, image.Data, PixelFormat.Rgba, name, uploadImmediately);
    }

    public static Font LoadFont(Stream stream, string name)
    {
        var library = new FreeTypeLibrary();
        var data = loadFontFromMemory(stream, name);
        unsafe
        {
            // uint spread = 8;
            // fixed (byte* moduleName   = "sdf\0"u8)
            // fixed (byte* propertyName = "spread\0"u8)
            // {
            //     FT.FT_Property_Set(library.Native, moduleName, propertyName, &spread);
            // }

            fixed (byte* dataPtr = data)
            {
                FT_FaceRec_* facePtr;
                var error = FT.FT_New_Memory_Face(
                    library.Native,
                    dataPtr,
                    data.Length,
                    0,
                    &facePtr
                );

                if (error != FT_Error.FT_Err_Ok) throw new InvalidOperationException($"Failed to load font from memory: {error}");

                var face = new FreeTypeFaceFacade(library, facePtr);

                var atlas = FontAtlas.Generate(face);
                library.Dispose();

                return new Font(name, 64, atlas);
            }
        }
    }

    private static byte[] loadFontFromMemory(Stream stream, string name)
    {
        byte[] fontData;

        if (stream is MemoryStream ms)
        {
            fontData = ms.ToArray();
        }
        else
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            fontData = memoryStream.ToArray();
        }

        return fontData;
    }
}
