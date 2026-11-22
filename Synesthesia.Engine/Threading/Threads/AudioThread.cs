// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Audio;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Threading.Threads;

public class AudioThread : ThreadRunner
{
    public override ThreadType Type => ThreadType.Audio;

    protected override Logger.LogCategory LoggerCategory => Logger.Audio;

    [Injected]
    private AudioManager audioManager = null!;

    protected override void ProcessFrame(FrameInfo frameInfo)
    {
        if (audioManager.CheckForDeviceChanges())
        {
            audioManager.UpdateAudioDevices();
            Logger.Verbose("New audio devices detected", Logger.Audio);
        }

        audioManager.UpdateSoundLifetimes();
    }

    protected override void OnThreadInit()
    {
        LoadFuture.Complete(this);
        InactiveUpdateRate.Value = ActiveUpdateRate.Value;

        audioManager.Initialize();
    }
}
