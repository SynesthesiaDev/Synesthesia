using Codon.IniTranscoder;
using Codon.IniTranscoder.Elements;
using Common.Logger;

namespace Synesthesia.Engine.Configuration;

public static class EngineConfiguration
{
    private const string path = "./engine.ini";

    private static ConfigurationFile current = ConfigurationFile.DEFAULT;

    public static bool DidMigrate = false;

    public static bool ShowDebugOverlay
    {
        get => current.ShowDebugOverlay;
        set => current = current with { ShowDebugOverlay = value };
    }

    public static bool LeftAltEscapesCursorConsume
    {
        get => current.LeftAltEscapesCursorConsume;
        set => current = current with { LeftAltEscapesCursorConsume = value };
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

    public static bool ReduceFpsWhenInactive
    {
        get => current.ReduceFpsWhenInactive;
        set => current = current with { ReduceFpsWhenInactive = value };
    }

    private static void update(ConfigurationFile newConfiguration)
    {
        current = newConfiguration;
        Save();
    }

    public static void Load()
    {
        if (!File.Exists(path))
        {
            File.Create(path).Close();
            File.WriteAllText(path, ConfigurationFile.VERSIONED_STRUCT_CODEC.Encode(IniTranscoder.INSTANCE, ConfigurationFile.DEFAULT).ToString());
        }
        else
        {
            var text = File.ReadAllText(path);
            var decoded = ConfigurationFile.VERSIONED_STRUCT_CODEC.Decode(IniTranscoder.INSTANCE, IniSection.Parse(text));
            current = decoded;
        }

        Logger.Verbose("Loaded engine configuration file", Logger.Io);
        Save();
    }

    public static void Save()
    {
        if (!File.Exists(path))
        {
            File.Create(path).Close();
            File.WriteAllText(path, ConfigurationFile.VERSIONED_STRUCT_CODEC.Encode(IniTranscoder.INSTANCE, current).ToString());
        }
        else
        {
            File.WriteAllText(path, ConfigurationFile.VERSIONED_STRUCT_CODEC.Encode(IniTranscoder.INSTANCE, current).ToString());
        }

        Logger.Verbose("Updated engine configuration file", Logger.Io);
    }
}
