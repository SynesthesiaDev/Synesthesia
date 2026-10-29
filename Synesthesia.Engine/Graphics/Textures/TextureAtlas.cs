// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Codon.Binary;
using Synesthesia.Common.Utils;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Graphics.Textures;

public class TextureAtlas : IDisposable, INamedResource
{
    public readonly int Width;
    public readonly int Height;
    public readonly Texture Texture;
    public readonly TextureData TextureData;
    public readonly Dictionary<int, TextureRegion> TextureRegions;

    public override string ToString() => $"TextureAtlas(Texture={Texture.ResourceName}, Width={Width}, Height={Height}, TextureRegions={TextureRegions.Count})";

    public Vector2 Size => new Vector2(Width, Height);

    public TextureAtlas(string name, int width, int height, Texture texture, Dictionary<int, TextureRegion> textureRegions)
    {
        ResourceName = name;
        Texture = texture;
        TextureData = texture.TextureData;
        Width = width;
        Height = height;
        TextureRegions = textureRegions;
        EngineStatistics.Increment(EngineStatistics.Type.TextureAtlases);
    }

    public TextureAtlas(string name, int width, int height, TextureData textureData, Dictionary<int, TextureRegion> textureRegions)
    {
        ResourceName = name;
        Texture = new Texture($"_atlas_{name}", textureData, uploadImmediately: true);
        Width = width;
        Height = height;
        TextureData = textureData;
        TextureRegions = textureRegions;
        EngineStatistics.Increment(EngineStatistics.Type.TextureAtlases);
    }

    public bool IsUploaded => Texture.IsUploaded;

    public static readonly IBinaryCodec<TextureAtlas> BINARY_CODEC = BinaryCodecs.For<TextureAtlas>()
        .Field(BinaryCodecs.STRING, a => a.ResourceName)
        .Field(BinaryCodecs.INT, a => a.Width)
        .Field(BinaryCodecs.INT, a => a.Height)
        .Field(TextureData.BINARY_CODEC, a => a.TextureData)
        .Field(BinaryCodecs.INT.MapTo(TextureRegion.BINARY_CODEC), a => a.TextureRegions)
        .Build((name, width, height, textureData, areas) => new TextureAtlas(name, width, height, textureData, areas));

    public TextureRegion GetRegion(int handle) => GetRegionOrNull(handle) ?? throw new InvalidOperationException($"No texture in atlas with handle {handle}");

    public TextureRegion? GetRegionOrNull(int handle) => TextureRegions.GetOrNullStruct(handle);

    public void Dispose()
    {
        Texture.Dispose();
        TextureRegions.Clear();
        EngineStatistics.Decrement(EngineStatistics.Type.TextureAtlases);
    }

    public string ResourceName { get; }
}
