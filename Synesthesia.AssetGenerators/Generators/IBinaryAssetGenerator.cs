// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Binary;
using DotNetty.Buffers;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Resources;

namespace Synesthesia.AssetGenerators.Generators;

public abstract class BinaryAssetGenerator<T> : IAssetGenerator<T>
{
    private readonly Dictionary<string, IByteBuffer> writtenAssets = new Dictionary<string, IByteBuffer>();

    public abstract IBinaryCodec<T> AssetCodec { get; }
    public abstract string Name { get; }

    private string path => $"./generated/{Name}";

    public void Run(IDictionary<string, T> assets)
    {
        Logger.Debug($"Writing {assets.Count} assets using {Name} to {path}..");
        var i = 0;
        foreach (var (name, asset) in assets)
        {
            i++;
            var buffer = Unpooled.Buffer();
            AssetCodec.Write(buffer, asset);
            writtenAssets[name] = buffer;
            Logger.Debug($"Encoded {i}/{assets.Count} assets..");
        }

        i = 0;
        foreach (var (name, asset) in writtenAssets)
        {
            i++;
            var filePath = Path.Join(path, $"{name}.{ResourceLoaders.FONT_ATLAS_FILE_EXT}");
            Directory.CreateDirectory(path);
            File.Create(filePath).Close();
            File.WriteAllBytes(filePath, asset.Array);
            Logger.Debug($"Written {i}/{assets.Count} assets..");
        }
    }
}
