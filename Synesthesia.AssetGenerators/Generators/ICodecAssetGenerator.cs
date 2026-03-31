// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;

namespace Synesthesia.AssetGenerators.Generators;

public interface ICodecAssetGenerator<T> : IAssetGenerator<T> where T : notnull
{
    Codec<T> AssetCodec { get; }
}
