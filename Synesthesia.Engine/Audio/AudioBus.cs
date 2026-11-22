// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass;
using ManagedBass.Mix;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Audio;

public class AudioBus : BassAudioHandler, IHasAudioHandle
{
    protected internal bool IsDisposed { get; private set; }

    private readonly List<AudioMixer> mixers = [];

    public string Name { get; set; }

    public int MixdownHandle { get; private set; }

    public IReadOnlyList<AudioMixer> Mixers => mixers;

    public int GetAudioHandle() => MixdownHandle;

    public int OutputHandle { get; private set; }

    public void AttachTo(IHasAudioHandle audioHandle)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var handle = audioHandle.GetAudioHandle();
        if (handle == 0) throw new ArgumentOutOfRangeException(nameof(audioHandle));

        if (handle == OutputHandle)
            return;

        if (OutputHandle != 0) BassMix.MixerRemoveChannel(MixdownHandle);

        if (!BassMix.MixerAddChannel(handle, MixdownHandle, BassFlags.Default))
            throw new InvalidOperationException($"Failed to route audio bus {Name} -> output: {Bass.LastError}");

        OutputHandle = handle;
    }

    public void UpdateSoundLifetimes()
    {
        for (int i = 0; i < mixers.Count; i++)
        {
            var mixer = mixers[i];
            mixer.UpdateSoundLifetimes();
        }
    }

    public float Panning
    {
        get
        {
            Bass.ChannelGetAttribute(MixdownHandle, ChannelAttribute.Pan, out var pan);
            return pan;
        }
        set
        {
            var sanitized = Math.Clamp(value, -1f, 1f);
            Bass.ChannelSetAttribute(MixdownHandle, ChannelAttribute.Pan, sanitized);
        }
    }


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

    public AudioBus(string name, int channels = 2)
    {
        Name = name;

        MixdownHandle = BassMix.CreateMixerStream(AudioManager.PLAYBACK_SAMPLE_RATE, channels, BassFlags.MixerNonStop | BassFlags.Float | BassFlags.Decode);

        if (MixdownHandle == 0)
            throw new InvalidOperationException($"Failed to create audio bus mixdown: {name}: {Bass.LastError}");

        AttachDspHandle(MixdownHandle);

        Bass.ChannelPlay(MixdownHandle);
        EngineStatistics.Increment(EngineStatistics.Type.AudioBuses);
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
        EngineStatistics.Decrement(EngineStatistics.Type.AudioBuses);
        IsDisposed = true;
    }
}
