using Common.Statistics;
using ManagedBass;
using ManagedBass.Mix;
using Synesthesia.Engine.Audio.Controls;

namespace Synesthesia.Engine.Audio;

public record AudioSampleInstance : IPlaybackAudioControl
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

    public AudioSampleInstance(AudioSample Sample, AudioMixer OwningAudioMixer, int StreamHandle)
    {
        this.Sample = Sample;
        this.OwningAudioMixer = OwningAudioMixer;
        this.StreamHandle = StreamHandle;

        EngineStatistics.AUDIO_SAMPLE_INSTANCES.Increment();
    }

    public float Volume
    {
        get => volume;
        set
        {
            volume = value;
            Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.Volume, volume);
        }
    }

    public AudioSample Sample { get; init; }
    public AudioMixer OwningAudioMixer { get; init; }
    public int StreamHandle { get; init; }

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
        BassMix.ChannelAddFlag(StreamHandle, BassFlags.MixerChanPause);
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
        EngineStatistics.AUDIO_SAMPLE_INSTANCES.Decrement();
    }

    public void Deconstruct(out AudioSample Sample, out AudioMixer OwningAudioMixer, out int StreamHandle)
    {
        Sample = this.Sample;
        OwningAudioMixer = this.OwningAudioMixer;
        StreamHandle = this.StreamHandle;
    }
}
