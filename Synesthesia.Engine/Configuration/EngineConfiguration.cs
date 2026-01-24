using Codon.Codec;
using Codon.IniTranscoder;
using Codon.IniTranscoder.Elements;
using Common.Logger;

namespace Synesthesia.Engine.Configuration;

public static class EngineConfiguration
{
    private const string path = "./engine.ini";

    private static RawConfigurationFile current = RawConfigurationFile.DEFAULT;

    public static bool ShowLogOverlay
    {
        get => current.ShowLogOverlay;
        set => current = current with { ShowLogOverlay = value };
    }

    public static bool ShowEngineStatistics
    {
        get => current.ShowEngineStatistics;
        set => current = current with { ShowEngineStatistics = value };
    }

    public static bool ShowFps
    {
        get => current.ShowFps;
        set => current = current with { ShowFps = value };
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
        bool ShowLogOverlay,
        bool ShowEngineStatistics,
        bool ShowFps,
        GarbageCollectionMode GarbageCollectionMode,
        ExecutionMode ExecutionMode,
        bool ExperimentalAudioWasapi
    )
    {
        public static readonly RawConfigurationFile DEFAULT = new(false, false, false, GarbageCollectionMode.Default, ExecutionMode.MultiThreaded, false);

        public static readonly StructCodec<RawConfigurationFile> CODEC = StructCodec.Of
        (
            "showLogOverlay", Codecs.Boolean, r => r.ShowLogOverlay,
            "showEngineStatistics", Codecs.Boolean, r => r.ShowEngineStatistics,
            "showFps", Codecs.Boolean, r => r.ShowFps,
            "garbageCollectionMode", Codecs.Enum<GarbageCollectionMode>(), r => r.GarbageCollectionMode,
            "executionMode", Codecs.Enum<ExecutionMode>(), r => r.ExecutionMode,
            "experimentalAudioWasapi", Codecs.Boolean, r => r.ExperimentalAudioWasapi,
            (showLogOverlay, showEngineStatistics, showFps, garbage, execution, wasapi) => new RawConfigurationFile(showLogOverlay, showEngineStatistics, showFps, garbage, execution, wasapi)
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