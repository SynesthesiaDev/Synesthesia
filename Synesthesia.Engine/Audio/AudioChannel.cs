using Common.Statistics;
using ManagedBass;
using ManagedBass.Mix;
using Synesthesia.Engine.Audio.Controls;

namespace Synesthesia.Engine.Audio;

public class AudioChannel : BassDspAudioHandler, IHasAudioHandle
{
    protected internal bool IsDisposed { get; private set; }

    private readonly List<AudioMixer> mixers = [];

    public string Name { get; set; }

    public int MixdownHandle { get; private set; }

    public IReadOnlyList<AudioMixer> Mixers => mixers;

    public int GetAudioHandle() => MixdownHandle;

    public int OutputHandle { get; private set; } = 0;

    public void AttachTo(IHasAudioHandle audioHandle)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var handle = audioHandle.GetAudioHandle();
        if (handle == 0) throw new ArgumentOutOfRangeException(nameof(audioHandle));

        if (handle == OutputHandle)
            return;

        if (OutputHandle != 0) BassMix.MixerRemoveChannel(MixdownHandle);

        if (!BassMix.MixerAddChannel(handle, MixdownHandle, BassFlags.Default))
            throw new InvalidOperationException($"Failed to route channel {Name} -> output: {Bass.LastError}");

        OutputHandle = handle;
    }

    public void UpdateSampleLifetimes() => mixers.ForEach(mixer => mixer.UpdateLifetimes());

    public override float Volume
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            Bass.ChannelGetAttribute(MixdownHandle, ChannelAttribute.Volume, out var v);
            return v;
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            Bass.ChannelSetAttribute(MixdownHandle, ChannelAttribute.Volume, value);
        }
    }

    public AudioChannel(string name, int channels = 2)
    {
        Name = name;

        MixdownHandle = BassMix.CreateMixerStream(AudioManager.PLAYBACK_SAMPLE_RATE, channels, BassFlags.MixerNonStop | BassFlags.Float | BassFlags.Decode);

        if (MixdownHandle == 0)
            throw new InvalidOperationException($"Failed to create channel mixdown: {name}: {Bass.LastError}");

        AttachDspHandle(MixdownHandle);

        Bass.ChannelPlay(MixdownHandle);
        EngineStatistics.AUDIO_CHANNELS.Increment();
    }

    public void AddMixer(AudioMixer mixer)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        mixers.Add(mixer);
        mixer.AttachTo(this);
    }

    public AudioMixer CreateMixer(string identifier)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var mixer = new AudioMixer(identifier, this);
        AddMixer(mixer);
        return mixer;
    }

    public new void Dispose()
    {
        if (IsDisposed) return;

        mixers.ForEach(m => m.Dispose());

        if (OutputHandle != 0)
            BassMix.MixerRemoveChannel(MixdownHandle);

        DisposeDspHandle(MixdownHandle);

        Bass.StreamFree(MixdownHandle);
        EngineStatistics.AUDIO_CHANNELS.Decrement();
        IsDisposed = true;
    }
}
