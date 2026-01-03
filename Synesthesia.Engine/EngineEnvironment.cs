using Common.Util;
using Synesthesia.Engine.Configuration;

namespace Synesthesia.Engine;

public class EngineEnvironment
{
    public static readonly bool DisableColoredLogs = EnvUtils.GetBool("DISABLE_COLORED_LOGS", false);

    public static readonly bool StartFullscreen = EnvUtils.GetBool("START_FULLSCREEN", false);
}