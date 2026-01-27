using Codon.Codec;
using Codon.IniTranscoder;
using Codon.IniTranscoder.Elements;
using Common.Logger;

namespace Synesthesia.Engine.Configuration;

public static class EngineConfiguration
{
    private const string path = "./engine.ini";

    private static RawConfigurationFile current = RawConfigurationFile.DEFAULT;

    public static bool ShowDebugOverlay
    {
        get => current.ShowDebugOverlay;
        set => current = current with { ShowDebugOverlay = value };
    }

    public static GarbageCollectionMode GarbageCollectionMode
    {
        get => current.GarbageCollectionMode;
        set => current = current with { GarbageCollectionMode = value };
    }

    public static ExecutionMode ExecutionMode
    {
        get => current.ExecutionMode;
        set => current = current with { ExecutionMode = value };
    }

    public static bool ExperimentalAudioWasapi
    {
        get => current.ExperimentalAudioWasapi;
        set => current = current with { ExperimentalAudioWasapi = value };
    }

    private static void update(RawConfigurationFile newConfiguration)
    {
        current = newConfiguration;
        Save();
    }

    private record RawConfigurationFile
    (
        bool ShowDebugOverlay,
        GarbageCollectionMode GarbageCollectionMode,
        ExecutionMode ExecutionMode,
        bool ExperimentalAudioWasapi
    )
    {
        public static readonly RawConfigurationFile DEFAULT = new(false, GarbageCollectionMode.Default, ExecutionMode.MultiThreaded, false);

        public static readonly StructCodec<RawConfigurationFile> CODEC = StructCodec.Of
        (
            "showDebugOverlay", Codecs.Boolean, r => r.ShowDebugOverlay,
            "garbageCollectionMode", Codecs.Enum<GarbageCollectionMode>(), r => r.GarbageCollectionMode,
            "executionMode", Codecs.Enum<ExecutionMode>(), r => r.ExecutionMode,
            "experimentalAudioWasapi", Codecs.Boolean, r => r.ExperimentalAudioWasapi,
            (showDebugOverlay, garbage, execution, wasapi) => new RawConfigurationFile(showDebugOverlay, garbage, execution, wasapi)
        );
    }

    public static void Load()
    {
        if (!File.Exists(path))
        {
            File.Create(path).Close();
            File.WriteAllText(path, RawConfigurationFile.CODEC.Encode(IniTranscoder.Instance, RawConfigurationFile.DEFAULT).ToString());
        }
        else
        {
            var text = File.ReadAllText(path);
            var decoded = RawConfigurationFile.CODEC.Decode(IniTranscoder.Instance, IniSection.Parse(text));
            current = decoded;
        }
        Logger.Verbose("Loaded engine configuration file", Logger.Io);
    }

    public static void Save()
    {
        if (!File.Exists(path))
        {
            File.Create(path).Close();
            File.WriteAllText(path, RawConfigurationFile.CODEC.Encode(IniTranscoder.Instance, current).GetAsValueOrThrow().ToString());
        }
        else
        {
            File.WriteAllText(path, RawConfigurationFile.CODEC.Encode(IniTranscoder.Instance, current).GetAsValueOrThrow().ToString());
        }
        Logger.Verbose("Updated engine configuration file", Logger.Io);
    }
}
