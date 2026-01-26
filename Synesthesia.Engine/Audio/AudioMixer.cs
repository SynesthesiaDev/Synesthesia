using Common.Statistics;
using ManagedBass;
using ManagedBass.Mix;
using Synesthesia.Engine.Audio.Controls;

namespace Synesthesia.Engine.Audio;

public class AudioMixer : IAudioControl, IHasAudioHandle
{
    protected internal bool IsDisposed { get; private set; }

    public readonly string Identifier;

    public AudioChannel OwningAudioChannel { get; private set; }

    private readonly Dictionary<IEffectParameter, int> activeEffects = new();

    private readonly List<AudioSampleInstance> activeInstances = [];

    public IReadOnlyList<AudioSampleInstance> ActiveInstances => activeInstances;

    public bool IsPaused { get; private set; } = false;

    public int OutputHandle { get; private set; } = 0;

    public int MixdownHandle { get; private set; }

    public int GetAudioHandle() => MixdownHandle;

    public float Volume
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            Bass.ChannelGetAttribute(MixdownHandle, ChannelAttribute.Volume, out var volume);
            return volume;
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            Bass.ChannelSetAttribute(MixdownHandle, ChannelAttribute.Volume, value);
        }
    }

    public void UpdateLifetimes()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        foreach (var instance in activeInstances
                     .ToList()
                     .Where(instance => Bass.ChannelIsActive(instance.StreamHandle) == PlaybackState.Stopped))
        {
            activeInstances.Remove(instance);
            instance.Dispose();
        }
    }

    public void AttachTo(IHasAudioHandle audioHandle)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (audioHandle is not AudioChannel audioChannel)
            throw new InvalidOperationException("Audio Handle must be Audio Channel!");

        var newOutputHandle = audioChannel.GetAudioHandle();
        if (newOutputHandle == 0) throw new ArgumentOutOfRangeException(nameof(audioHandle));

        if (newOutputHandle == OutputHandle)
            return;

        if (OutputHandle != 0)
            BassMix.MixerRemoveChannel(MixdownHandle);

        OwningAudioChannel = audioChannel;

        if (!BassMix.MixerAddChannel(OwningAudioChannel.MixdownHandle, MixdownHandle, BassFlags.Default))
            throw new InvalidOperationException($"Failed to route mixer '{Identifier}' -> channel '{OwningAudioChannel.Name}': {Bass.LastError}");

        OutputHandle = newOutputHandle;
    }

    public AudioMixer(string identifier, AudioChannel owningAudioChannel)
    {
        Identifier = identifier;
        OwningAudioChannel = owningAudioChannel;

        MixdownHandle = BassMix.CreateMixerStream(AudioManager.PLAYBACK_SAMPLE_RATE, 2, BassFlags.MixerNonStop | BassFlags.Float | BassFlags.Decode);
        if (MixdownHandle == 0) throw new InvalidOperationException($"Failed to create mixer with identifier '{Identifier}': {Bass.LastError}");

        EngineStatistics.AUDIO_MIXERS.Increment();

        Bass.ChannelPlay(MixdownHandle);
    }

    public void Pause()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        activeInstances.ForEach(i => i.Pause());
        IsPaused = true;
    }

    public void Resume()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        activeInstances.ForEach(i => i.Resume());
        IsPaused = false;
    }

    public void AddEffect(IEffectParameter effect, int priority)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var handle = Bass.ChannelSetFX(MixdownHandle, effect.FXType, priority);
        Bass.FXSetParameters(handle, effect);

        activeEffects[effect] = handle;
    }

    public void RemoveEffect(IEffectParameter effect)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (!activeEffects.Remove(effect, out var handle)) return;

        Bass.ChannelRemoveFX(MixdownHandle, handle);
    }

    public void UpdateEffect(IEffectParameter effect)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (!activeEffects.TryGetValue(effect, out var handle))
            return;

        Bass.FXSetParameters(handle, effect);
    }

    public AudioSampleInstance Play(AudioSample sample)
    {
        return Play(sample, sample.DefaultVolume.Random(), sample.DefaultPitch.Random());
    }

    public AudioSampleInstance Play(AudioSample sample, float volume)
    {
        return Play(sample, volume, sample.DefaultPitch.Random());
    }

    public AudioSampleInstance Play(AudioSample sample, float volume, float pitch)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var byteArr = sample.Data.ToArray();

        var streamHandle = Bass.CreateStream(byteArr, 0, byteArr.Length, BassFlags.Decode | BassFlags.Prescan);

        if (streamHandle == 0)
            throw new InvalidOperationException($"Failed to create stream for a sample: {Bass.LastError}");

        if (sample.Looping) Bass.ChannelFlags(streamHandle, BassFlags.Loop, BassFlags.Loop);

        if (!BassMix.MixerAddChannel(MixdownHandle, streamHandle, BassFlags.Default))
        {
            Bass.StreamFree(streamHandle);
            throw new InvalidOperationException($"Failed to add stream to mixer for a sample: {Bass.LastError}");
        }

        var instance = new AudioSampleInstance(sample, this, streamHandle);

        instance.Volume = volume;
        instance.Pitch = pitch;

        activeInstances.Add(instance);

        if (IsPaused)
        {
            instance.Pause();
        }
        else
        {
            Bass.ChannelPlay(streamHandle);
        }

        return instance;
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        foreach (var fxHandle in activeEffects.Values)
            Bass.ChannelRemoveFX(MixdownHandle, fxHandle);

        activeEffects.Clear();

        if (MixdownHandle != 0)
        {
            if (OutputHandle != 0)
                BassMix.MixerRemoveChannel(MixdownHandle);

            Bass.StreamFree(MixdownHandle);
            MixdownHandle = 0;
            OutputHandle = 0;
        }

        IsDisposed = true;
        EngineStatistics.AUDIO_MIXERS.Decrement();
    }
}
