// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Binary;
using Synesthesia.Engine.Graphics.Textures;

namespace Synesthesia.AssetGenerators.Generators;

public class FontAtlasGenerator : BinaryAssetGenerator<Font>
{
    public override string Name => "font_atlas";

    public override IBinaryCodec<Font> AssetCodec => Font.BINARY_CODEC;

}
