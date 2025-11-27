using System.ComponentModel;

namespace Synesthesia.Engine.Configuration;

public enum ExecutionMode
{
    [Description("Single-Threaded")] SingleThreaded,
    [Description("Multi-Threaded")] MultiThreaded
}