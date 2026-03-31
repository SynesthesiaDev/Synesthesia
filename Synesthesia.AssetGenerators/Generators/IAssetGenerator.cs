// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.AssetGenerators.Generators;

public interface IAssetGenerator<T>
{
    string Name { get; }

    void Run(IDictionary<string, T> assets);

}
