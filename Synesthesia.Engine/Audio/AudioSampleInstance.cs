using ManagedBass;
using ManagedBass.Mix;
using Synesthesia.Engine.Audio.Controls;

namespace Synesthesia.Engine.Audio;

public record AudioSampleInstance(AudioSample Sample, AudioMixer OwningAudioMixer, int StreamHandle) : IPlaybackAudioControl
{
    public float Progress
    {
        get
        {
            var length = Bass.ChannelGetLength(StreamHandle);
            var position = Bass.ChannelGetPosition(StreamHandle);
            
            if (length <= 0) return 0f;
            
            return position < 0 ? 0f : Math.Clamp((float)position / length, 0f, 1f);
        }
    }

    public bool IsPaused { get; private set; } = false;

    private float pitch = 1f;

    public float Pitch
    {
        get => pitch;
        set
        {
            pitch = value;
            Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.Frequency, AudioManager.PLAYBACK_SAMPLE_RATE * pitch);
        }
    }

    private float volume = 1f;

    public float Volume
    {
        get => volume;
        set
        {
            volume = value;
            Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.Volume, volume);
        }
    }

    public void Seek(double seconds)
    {
        var bytes = Bass.ChannelSeconds2Bytes(StreamHandle, Math.Max(0.0, seconds));
        Seek(bytes);
    }

    public void Seek(long bytes)
    {
        Bass.ChannelSetPosition(StreamHandle, bytes);
    }

    public void Pause()
    {
        Bass.ChannelAddFlag(StreamHandle, BassFlags.MixerChanPause);
        IsPaused = true;
    }

    public void Resume()
    {
        BassMix.ChannelRemoveFlag(StreamHandle, BassFlags.MixerChanPause);
        IsPaused = false;
    }

    public void Restart()
    {
        Seek(Sample.RestartPoint);
        IsPaused = false;
    }
    
    public void Dispose()
    {
        BassMix.MixerRemoveChannel(StreamHandle);
        Bass.StreamFree(StreamHandle);
    }

}