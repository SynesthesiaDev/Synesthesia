// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;
using Codon.Codec.Versioned;
using Codon.IniTranscoder.Elements;
using Common.Logger;

namespace Synesthesia.Engine.Configuration;

public record RawConfigurationFile(
    bool ShowDebugOverlay,
    GarbageCollectionMode GarbageCollectionMode,
    ExecutionMode ExecutionMode,
    bool ExperimentalAudioWasapi,
    bool LeftAltEscapesCursorConsume,
    bool RawInput
)
{
    public static readonly RawConfigurationFile DEFAULT = new(
        ShowDebugOverlay: false,
        GarbageCollectionMode: GarbageCollectionMode.Default,
        ExecutionMode: ExecutionMode.MultiThreaded,
        ExperimentalAudioWasapi: true,
        LeftAltEscapesCursorConsume: true,
        RawInput: false
    );

    private static readonly StructCodec<RawConfigurationFile> codec = StructCodec.Of
    (
        "showDebugOverlay", Codecs.BOOLEAN, r => r.ShowDebugOverlay,
        "garbageCollectionMode", Codecs.Enum<GarbageCollectionMode>(), r => r.GarbageCollectionMode,
        "executionMode", Codecs.Enum<ExecutionMode>(), r => r.ExecutionMode,
        "experimentalAudioWasapi", Codecs.BOOLEAN, r => r.ExperimentalAudioWasapi,
        "leftAltEscapesCursorConsume", Codecs.BOOLEAN, r => r.LeftAltEscapesCursorConsume,
        "rawInput", Codecs.BOOLEAN, r => r.RawInput,
        (showDebugOverlay, garbage, execution, wasapi, leftAltToEscapeCursorConsume, rawInput) => new RawConfigurationFile(showDebugOverlay, garbage, execution, wasapi, leftAltToEscapeCursorConsume, rawInput)
    );

    public static readonly VersionedStructCodec<RawConfigurationFile> VERSIONED_STRUCT_CODEC = new()
    {
        CurrentSchemaVersion = 1,
        InnerCodec = codec,
        SchemaMigrationRegistry = SchemaMigrationRegistry.Builder()
            .For<IIniElement>(migrations =>
            {
                migrations.Add(1, (transcoder, _, output) =>
                {
                    Logger.Verbose("Migrating config to schema 1");
                    EngineConfiguration.DidMigrate = true;
                    output.Put("rawInput", transcoder.EncodeBool(DEFAULT.RawInput));
                });
            })
    };
}
