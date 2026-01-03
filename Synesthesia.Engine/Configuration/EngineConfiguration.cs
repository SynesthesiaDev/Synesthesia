using Codon.Codec;
using Codon.IniTranscoder;
using Codon.IniTranscoder.Elements;
using Common.Logger;

namespace Synesthesia.Engine.Configuration;

public static class EngineConfiguration
{
    private const string Path = "./engine.ini";

    private static RawConfigurationFile Current = RawConfigurationFile.Default;

    public static bool ShowLogOverlay
    {
        get => Current.ShowLogOverlay;
        set => Current = Current with { ShowLogOverlay = value };
    }

    public static bool ShowEngineStatistics
    {
        get => Current.ShowEngineStatistics;
        set => Current = Current with { ShowEngineStatistics = value };
    }

    public static bool ShowFps
    {
        get => Current.ShowFps;
        set => Current = Current with { ShowFps = value };
    }

    public static GarbageCollectionMode GarbageCollectionMode
    {
        get => Current.GarbageCollectionMode;
        set => Current = Current with { GarbageCollectionMode = value };
    }

    public static ExecutionMode ExecutionMode
    {
        get => Current.ExecutionMode;
        set => Current = Current with { ExecutionMode = value };
    }

    public static bool ExperimentalAudioWasapi
    {
        get => Current.ExperimentalAudioWasapi;
        set => Current = Current with { ExperimentalAudioWasapi = value };
    }

    private static void Update(RawConfigurationFile newConfiguration)
    {
        Current = newConfiguration;
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
        public static readonly RawConfigurationFile Default = new(false, false, false, GarbageCollectionMode.Default, ExecutionMode.MultiThreaded, false);

        public static readonly StructCodec<RawConfigurationFile> Codec = StructCodec.Of
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
        if (!File.Exists(Path))
        {
            File.Create(Path).Close();
            File.WriteAllText(Path, RawConfigurationFile.Codec.Encode(IniTranscoder.Instance, RawConfigurationFile.Default).ToString());
        }
        else
        {
            var text = File.ReadAllText(Path);
            var decoded = RawConfigurationFile.Codec.Decode(IniTranscoder.Instance, IniSection.Parse(text));
            Current = decoded;
        }
        Logger.Verbose("Loaded engine configuration file", Logger.IO);
    }

    public static void Save()
    {
        if (!File.Exists(Path))
        {
            File.Create(Path).Close();
            File.WriteAllText(Path, RawConfigurationFile.Codec.Encode(IniTranscoder.Instance, Current).GetAsValueOrThrow().ToString());
        }
        else
        {
            File.WriteAllText(Path, RawConfigurationFile.Codec.Encode(IniTranscoder.Instance, Current).GetAsValueOrThrow().ToString());
        }
        Logger.Verbose("Updated engine configuration file", Logger.IO);
    }
}