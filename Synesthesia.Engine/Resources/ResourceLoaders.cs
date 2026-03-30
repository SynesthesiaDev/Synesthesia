// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using FreeTypeSharp;
using Silk.NET.OpenGL;
using StbImageSharp;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Graphics.Textures;
using Texture = Synesthesia.Engine.Graphics.Textures.Texture;

namespace Synesthesia.Engine.Resources;

public static class ResourceLoaders
{
    public const string TEXTURE_ATLAS_FILE_EXT = "txa";
    public const string FONT_ATLAS_FILE_EXT = "fna";

    public static Texture LoadTexture(Stream stream, bool uploadImmediately = false)
    {
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var textureData = new TextureData(image.Width, image.Height, image.Data, PixelFormat.Rgba);
        return new Texture(textureData, uploadImmediately);
    }

    public static TextureAtlas LoadFromTextureAtlasFile(Stream stream) => TextureAtlas.BINARY_CODEC.Read(stream.ToByteBuffer());

    public static FontAtlas LoadFromFontAtlasFile(Stream stream) => FontAtlas.BINARY_CODEC.Read(stream.ToByteBuffer());

    public static Font LoadFont(Stream stream, string name)
    {
        var library = new FreeTypeLibrary();
        var data = stream.ToByteArray();
        unsafe
        {
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
}
