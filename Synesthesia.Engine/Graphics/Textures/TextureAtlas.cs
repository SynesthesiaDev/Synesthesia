// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Codon.Binary;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Graphics.Textures;

public class TextureAtlas : IDisposable
{
    public readonly int Width;
    public readonly int Height;
    public readonly Texture Texture;
    public readonly Dictionary<int, TextureRegion> TextureRegions;

    public override string ToString() => $"TextureAtlas(Texture={Texture}, Width={Width}, Height={Height}, TextureRegions={TextureRegions.Count})";

    public Vector2 Size => new Vector2(Width, Height);

    public TextureAtlas(int width, int height, Texture texture, Dictionary<int, TextureRegion> textureRegions)
    {
        Texture = texture;
        Width = width;
        Height = height;
        TextureRegions = textureRegions;
        EngineStatistics.Increment(EngineStatistics.Type.TextureAtlases);
    }

    public bool IsUploaded => Texture.IsUploaded;

    public static readonly IBinaryCodec<TextureAtlas> BINARY_CODEC = BinaryCodecs.For<TextureAtlas>()
        .Field(BinaryCodecs.INT, a => a.Width)
        .Field(BinaryCodecs.INT, a => a.Height)
        .Field(TextureData.BINARY_CODEC.Transform<Texture>(texture => texture.TextureData, textureData => new Texture(textureData, true)), a => a.Texture)
        .Field(BinaryCodecs.INT.MapTo(TextureRegion.BINARY_CODEC), a => a.TextureRegions)
        .Build((width, height, texture, areas) => new TextureAtlas(width, height, texture, areas));

    public TextureRegion GetRegion(int handle) => GetRegionOrNull(handle) ?? throw new InvalidOperationException($"No texture in atlas with handle {handle}");

    public TextureRegion? GetRegionOrNull(int handle) => TextureRegions.GetOrNullStruct(handle);

    public void Dispose()
    {
        Texture.Dispose();
        TextureRegions.Clear();
        EngineStatistics.Decrement(EngineStatistics.Type.TextureAtlases);
    }
}
