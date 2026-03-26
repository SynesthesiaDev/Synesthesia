// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Extensions;

namespace Synesthesia.Engine.Graphics.Textures;

public class TextureAtlas(int width, int height, Texture texture, IDictionary<int, TextureRegion> textureRegions) : IDisposable
{
    public readonly Texture AtlasTexture = texture;
    public readonly int Width = width;
    public readonly int Height = height;

    public readonly IDictionary<int, TextureRegion> TextureRegions = textureRegions;

    public bool IsUploaded => AtlasTexture.IsUploaded;

    public TextureRegion GetRegion(int handle) => GetRegionOrNull(handle) ?? throw new InvalidOperationException($"No texture in atlas with handle {handle}");

    public TextureRegion? GetRegionOrNull(int handle) => TextureRegions.GetOrNullStruct(handle);

    public void Dispose()
    {
        AtlasTexture.Dispose();
        TextureRegions.Clear();
    }
}
