using Common.Util;

namespace Synesthesia.Engine;

public class EngineEnvironment
{
    public static readonly bool DISABLE_COLORED_LOGS = EnvUtils.GetBool("DISABLE_COLORED_LOGS", false);

    public static readonly bool START_FULLSCREEN = EnvUtils.GetBool("START_FULLSCREEN", false);
}