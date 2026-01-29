using Common.Logger;
using Common.Statistics;
using ManagedBass;
using Synesthesia.Engine.Audio;
using Synesthesia.Engine.Graphics;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Threading.Runners;

public class AudioThreadRunner(ThreadType type) : ThreadRunner(type)
{
    protected override Logger.LogCategory GetLoggerCategory() => Logger.Audio;

    private Game game = null!;

    private AudioManager audioManager = null!;

    protected override void OnThreadInit(Game game)
    {
        game.AudioManager.Initialize();
        this.game = game;
        audioManager = this.game.AudioManager;
    }

    protected override void OnLoadComplete(Game game)
    {

    }

    private long frameCount;

    protected override void OnLoop(FrameInfo frameInfo)
    {
        if (frameCount++ % 1000 == 0)
        {
            EngineStatistics.BASS_CPU.Update(_ => Bass.CPUUsage.FloorToDecimalDigits(2));
        }

        if (game.AudioManager.CheckForDeviceChanges())
        {
            game.AudioManager.UpdateAudioDevices();
            Logger.Verbose("New audio devices detected", Logger.Audio);
        }

        audioManager.UpdateSampleLifetimes();
    }
}
