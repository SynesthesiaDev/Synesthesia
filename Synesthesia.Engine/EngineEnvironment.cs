using Common.Util;
using Synesthesia.Engine.Configuration;

namespace Synesthesia.Engine;

public class EngineEnvironment
{
    public static readonly RendererType GraphicsRendererType = EnvUtils.GetEnum("GRAPHICS_RENDERER", RendererType.Automatic);

    public static readonly GarbageCollectionMode GarbageCollector = EnvUtils.GetEnum("GARBAGE_COLLECTOR", GarbageCollectionMode.Default);

    public static readonly ExecutionMode ExecutionMode = EnvUtils.GetEnum("EXECUTION_MODE", ExecutionMode.MultiThreaded);

    public static readonly bool DisableColoredLogs = EnvUtils.GetBool("DISABLE_COLORED_LOGS", false);
}