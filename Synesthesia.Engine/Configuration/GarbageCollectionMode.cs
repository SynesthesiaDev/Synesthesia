using System.ComponentModel;

namespace Synesthesia.Engine.Configuration;

public enum GarbageCollectionMode
{
    [Description("Default")] Default,
    [Description("Satori (Experimental)")] Satori
}