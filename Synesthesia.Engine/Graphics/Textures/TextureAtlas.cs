// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Graphics.Textures;

public class TextureAtlas : IDisposable
{
    public readonly Texture Texture;
    public readonly int Width;
    public readonly int Height;

    public readonly Vector2 Size;

    public readonly IDictionary<int, TextureRegion> TextureRegions;

    public TextureAtlas(int width, int height, Texture texture, IDictionary<int, TextureRegion> textureRegions)
    {
        Texture = texture;
        Width = width;
        Height = height;
        Size = new Vector2(width, height);
        TextureRegions = textureRegions;
        EngineStatistics.Increment(EngineStatistics.Type.TextureAtlases);
    }

    public bool IsUploaded => Texture.IsUploaded;

    public TextureRegion GetRegion(int handle) => GetRegionOrNull(handle) ?? throw new InvalidOperationException($"No texture in atlas with handle {handle}");

    public TextureRegion? GetRegionOrNull(int handle) => TextureRegions.GetOrNullStruct(handle);

    public void Dispose()
    {
        Texture.Dispose();
        TextureRegions.Clear();
        EngineStatistics.Decrement(EngineStatistics.Type.TextureAtlases);
    }
}
