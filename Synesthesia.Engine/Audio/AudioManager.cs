using Common.Logger;
using ManagedBass;

namespace Synesthesia.Engine.Audio;

public static class AudioManager
{
    public const int PLAYBACK_SAMPLE_RATE = 8000;
    public const int BASS_INTERNAL_DEVICE_COUNT = 2;

    private const int bass_default_device = 1;

    public static void Initialize()
    {
        Logger.Debug($"Initializing BASS {Bass.Version}", Logger.AUDIO);
        if (Bass.Init())
        {
            Logger.Debug("BASS Initialized", Logger.AUDIO);

        }
    }
}