// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass;
using ManagedBass.Mix;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Audio;

public class SoundInstance : IPlaybackAudioControl
{
    protected internal bool IsDisposed { get; private set; }

    public float Progress
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            var length = Bass.ChannelGetLength(StreamHandle);
            var position = Bass.ChannelGetPosition(StreamHandle);

            if (length <= 0) return 0f;

            return position < 0 ? 0f : Math.Clamp((float)position / length, 0f, 1f);
        }
    }

    public bool IsPaused { get; private set; }

    public float Pitch
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            field = value;
            Bass.ChannelGetInfo(StreamHandle, out var info);
            Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.Frequency, info.Frequency * field);
        }
    } = 1f;

    public float Panning
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            Bass.ChannelGetAttribute(StreamHandle, ChannelAttribute.Pan, out var pan);
            return pan;
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.Pan, Math.Clamp(value, -1f, 1f));
        }
    }

    public SoundInstance(Sound sound, AudioMixer owningAudioMixer, int streamHandle)
    {
        Sound = sound;
        OwningAudioMixer = owningAudioMixer;
        StreamHandle = streamHandle;

        EngineStatistics.Increment(EngineStatistics.Type.PlayingSounds);
    }

    public float Volume
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            field = value;
            Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.Volume, field);
        }
    } = 1f;

    public Sound Sound { get; init; }
    public AudioMixer OwningAudioMixer { get; init; }
    public int StreamHandle { get; init; }

    public void Seek(double seconds)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var bytes = Bass.ChannelSeconds2Bytes(StreamHandle, Math.Max(0.0, seconds));
        Seek(bytes);
    }

    public void Seek(long bytes)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        Bass.ChannelSetPosition(StreamHandle, bytes);
    }

    public void Pause()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        BassMix.ChannelAddFlag(StreamHandle, BassFlags.MixerChanPause);
        IsPaused = true;
    }

    public void Resume()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        BassMix.ChannelRemoveFlag(StreamHandle, BassFlags.MixerChanPause);
        IsPaused = false;
    }

    public void Restart()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        Seek(Sound.RestartPoint);
        IsPaused = false;
    }

    public void Stop()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        Bass.ChannelStop(StreamHandle);
    }

    public void Dispose()
    {
        if(IsDisposed) return;
        IsDisposed = true;

        BassMix.MixerRemoveChannel(StreamHandle);
        Bass.StreamFree(StreamHandle);
        EngineStatistics.Decrement(EngineStatistics.Type.PlayingSounds);
    }
}
