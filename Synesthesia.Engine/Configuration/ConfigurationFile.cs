// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;
using Codon.Codec.Versioned;
using Codon.IniTranscoder.Elements;
using Common.Logger;

namespace Synesthesia.Engine.Configuration;

public record ConfigurationFile(
    bool ShowDebugOverlay,
    GarbageCollectionMode GarbageCollectionMode,
    ExecutionMode ExecutionMode,
    bool ExperimentalAudioWasapi,
    bool LeftAltEscapesCursorConsume,
    bool RawInput,
    bool ReduceFpsWhenInactive
)
{
    public static readonly ConfigurationFile DEFAULT = new(
        ShowDebugOverlay: false,
        GarbageCollectionMode: GarbageCollectionMode.Default,
        ExecutionMode: ExecutionMode.MultiThreaded,
        ExperimentalAudioWasapi: true,
        LeftAltEscapesCursorConsume: true,
        RawInput: false,
        ReduceFpsWhenInactive: true
    );

    private static readonly StructCodec<ConfigurationFile> codec = StructCodec.Of
    (
        "showDebugOverlay", Codecs.BOOLEAN, r => r.ShowDebugOverlay,
        "garbageCollectionMode", Codecs.Enum<GarbageCollectionMode>(), r => r.GarbageCollectionMode,
        "executionMode", Codecs.Enum<ExecutionMode>(), r => r.ExecutionMode,
        "experimentalAudioWasapi", Codecs.BOOLEAN, r => r.ExperimentalAudioWasapi,
        "leftAltEscapesCursorConsume", Codecs.BOOLEAN, r => r.LeftAltEscapesCursorConsume,
        "rawInput", Codecs.BOOLEAN, r => r.RawInput,
        "reduceFpsWhenInactive", Codecs.BOOLEAN, r => r.ReduceFpsWhenInactive,
        (showDebugOverlay, garbage, execution, wasapi, leftAltToEscapeCursorConsume, rawInput, reduceFps) => new ConfigurationFile(showDebugOverlay, garbage, execution, wasapi, leftAltToEscapeCursorConsume, rawInput, reduceFps)
    );

    public static readonly VersionedStructCodec<ConfigurationFile> VERSIONED_STRUCT_CODEC = new()
    {
        CurrentSchemaVersion = 2,
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

                migrations.Add(2, (transcoder, _, output) =>
                {
                    Logger.Verbose("Migrating config to schema 2");
                    EngineConfiguration.DidMigrate = true;
                    output.Put("reduceFpsWhenInactive", transcoder.EncodeBool(DEFAULT.ReduceFpsWhenInactive));
                });
            })
    };
}
