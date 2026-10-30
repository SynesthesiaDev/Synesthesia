// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text;
using FreeTypeSharp;
using Silk.NET.OpenGL;
using StbImageSharp;
using Synesthesia.Common.Utils;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Parsers.Model;
using Shader = Synesthesia.Engine.Graphics.Shader;
using StringUtil = Synesthesia.Engine.Util.StringUtil;
using Texture = Synesthesia.Engine.Graphics.Textures.Texture;

namespace Synesthesia.Engine.Resources;

public static class ResourceLoaders
{
    public const string TEXTURE_ATLAS_FILE_EXT = "txa";
    public const string FONT_ATLAS_FILE_EXT = "fna";

    public static Texture LoadTexture(Stream stream, string name, bool uploadImmediately = false)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);

        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var textureData = new TextureData(image.Width, image.Height, image.Data, PixelFormat.Rgba);
        return new Texture(StringUtil.FileName(name), textureData, uploadImmediately);
    }

    public static Mesh LoadMesh(Stream stream, string name) => ObjFileParser.Parse(stream).ToMesh(name);

    public static Shader LoadShader(Stream stream, string name)
    {
        var array = stream.ToByteArray();
        var text = Encoding.UTF8.GetString(array);
        var lines = text.Split("\n");

        string? vertex = null;
        string? fragment = null;
        ShaderKind? kind = null;

        bool vertexMode = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("#pragma kind", StringComparison.OrdinalIgnoreCase))
            {
                var parsedKind = line.Split("#pragma kind ")[1];
                kind = EnumExtensions.Parse<ShaderKind>(parsedKind);
                continue;
            }

            if (line.StartsWith("#pragma vertex", StringComparison.OrdinalIgnoreCase))
            {
                vertexMode = true;
                continue;
            }

            if (line.StartsWith("#pragma fragment", StringComparison.OrdinalIgnoreCase))
            {
                vertexMode = false;
                continue;
            }

            if (vertexMode)
                vertex += $"{line}\n";
            else
                fragment += $"{line}\n";
        }

        if (vertex == null) throw new InvalidOperationException($"Vertex shader code for {name} not found. Make sure to specify `#pragma vertex` before vertex code section");
        if (fragment == null) throw new InvalidOperationException($"Fragment shader code for {name} not found, Make sure to specify `#pragma fragment` before fragment code section");
        if (kind == null) throw new InvalidOperationException($"Shader kind for {name} not found, make sure to specify it with `#pragma kind ('Two' for 2D or 'Three' for 3D)`");

        return new Shader(name, kind.Value, vertex, fragment);
    }

    public static TextureAtlas LoadFromTextureAtlasFile(Stream stream) => TextureAtlas.BINARY_CODEC.Read(stream.ToByteBuffer());

    public static Font LoadFontFromAtlas(Stream stream, string name)
    {
        var font = Font.BINARY_CODEC.Read(stream.ToByteBuffer());
        return font;
    }

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

                var atlas = FontAtlas.Generate($"atlas_{StringUtil.FileName(name)}", face);
                library.Dispose();

                return new Font(64, atlas);
            }
        }
    }
}
