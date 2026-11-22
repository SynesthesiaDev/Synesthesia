// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass;
using ManagedBass.Mix;
using Synesthesia.Engine.Util.Pooling;
using Synesthesia.Engine.Util.Statistics;
using Synesthesia.Utils.Extensions;

namespace Synesthesia.Engine.Audio;

public class AudioMixer : BassAudioHandler, IHasAudioHandle
{
    protected internal bool IsDisposed { get; private set; }

    public readonly string Identifier;

    public AudioBus OwningAudioBus { get; private set; }

    private readonly Dictionary<IAudioEffect, int> activeEffects = new();

    private readonly List<SoundInstance> activeInstances = [];

    public IReadOnlyList<SoundInstance> ActiveInstances => activeInstances;

    public bool IsPaused { get; private set; }

    public int OutputHandle { get; private set; }

    public int MixdownHandle { get; private set; }

    public int GetAudioHandle() => MixdownHandle;

    public float Panning
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            Bass.ChannelGetAttribute(MixdownHandle, ChannelAttribute.Pan, out var pan);
            return pan;
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            Bass.ChannelSetAttribute(MixdownHandle, ChannelAttribute.Pan, Math.Clamp(value, -1f, 1f));
        }
    }

    public override float Volume
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

    public void UpdateSoundLifetimes()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (activeInstances.IsEmpty()) return;

        Snapshot<SoundInstance> snapshot;
        lock (activeInstances)
        {
            snapshot = Snapshot.Rent(activeInstances);
        }

        try
        {
            foreach (ref SoundInstance instance in snapshot.Span)
            {
                if (Bass.ChannelIsActive(instance.StreamHandle) != PlaybackState.Stopped) continue;

                activeInstances.Remove(instance);
                instance.Dispose();
            }
        }
        finally
        {
            snapshot.Dispose();
        }
    }

    public void AttachTo(IHasAudioHandle audioHandle)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (audioHandle is not AudioBus audioChannel)
            throw new InvalidOperationException("Audio Handle must be Audio Bus!");

        var newOutputHandle = audioChannel.GetAudioHandle();
        if (newOutputHandle == 0) throw new ArgumentOutOfRangeException(nameof(audioHandle));

        if (newOutputHandle == OutputHandle)
            return;

        if (OutputHandle != 0)
            BassMix.MixerRemoveChannel(MixdownHandle);

        OwningAudioBus = audioChannel;

        if (!BassMix.MixerAddChannel(OwningAudioBus.MixdownHandle, MixdownHandle, BassFlags.Default))
            throw new InvalidOperationException($"Failed to route mixer '{Identifier}' -> audio bus '{OwningAudioBus.Name}': {Bass.LastError}");

        OutputHandle = newOutputHandle;
    }

    public AudioMixer(string identifier, AudioBus owningAudioBus)
    {
        Identifier = identifier;
        OwningAudioBus = owningAudioBus;

        MixdownHandle = BassMix.CreateMixerStream(AudioManager.PLAYBACK_SAMPLE_RATE, 2, BassFlags.MixerNonStop | BassFlags.Float | BassFlags.Decode);
        if (MixdownHandle == 0) throw new InvalidOperationException($"Failed to create mixer with identifier '{Identifier}': {Bass.LastError}");

        EngineStatistics.Increment(EngineStatistics.Type.AudioMixers);

        Bass.Configure(Configuration.FloatDSP, NewValue: true);

        AttachDspHandle(MixdownHandle);

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

    public void AddEffect(IAudioEffect effect)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        effect.AttachTo(this);

        activeEffects[effect] = effect.GetAudioHandle();
    }

    public void RemoveEffect(IAudioEffect effect)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (!activeEffects.Remove(effect, out _)) return;

        effect.Detach();
    }

    public SoundInstance Play(Sound sound)
    {
        return Play(sound, sound.DefaultVolume.Random(), sound.DefaultPitch.Random());
    }

    public SoundInstance Play(Sound sound, float volume)
    {
        return Play(sound, volume, sound.DefaultPitch.Random());
    }

    public SoundInstance Play(Sound sound, float volume, float pitch)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        int streamHandle;
        bool isSampleChannel = sound.PlaybackMode == PlaybackMode.Decoded;

        if (isSampleChannel)
        {
            streamHandle = Bass.SampleGetChannel(sound.Buffer.PcmData.EnsureSample(), BassFlags.Decode);
        }
        else
        {
            var bytes = sound.Buffer.Data;
            streamHandle = Bass.CreateStream(bytes, 0, bytes.Length, BassFlags.Decode | BassFlags.Prescan);
        }

        if (streamHandle == 0)
            throw new InvalidOperationException($"Failed to create stream for a sound: {Bass.LastError}");

        if (sound.Looping) Bass.ChannelFlags(streamHandle, BassFlags.Loop, BassFlags.Loop);

        if (!BassMix.MixerAddChannel(MixdownHandle, streamHandle, BassFlags.Default))
        {
            Bass.StreamFree(streamHandle);
            throw new InvalidOperationException($"Failed to add stream to mixer for a sound: {Bass.LastError}");
        }

        var instance = new SoundInstance(sound, this, streamHandle);

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
            ensureEffects();
        }

        return instance;
    }

    private void ensureEffects()
    {
        foreach (var (effect, _) in activeEffects) effect.ApplyToStream();
    }

    public new void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;

        foreach (var fxHandle in activeEffects.Values)
            Bass.ChannelRemoveFX(MixdownHandle, fxHandle);

        activeEffects.Clear();

        DisposeDspHandle(MixdownHandle);

        if (MixdownHandle != 0)
        {
            if (OutputHandle != 0)
                BassMix.MixerRemoveChannel(MixdownHandle);

            Bass.StreamFree(MixdownHandle);
            MixdownHandle = 0;
            OutputHandle = 0;
        }

        EngineStatistics.Decrement(EngineStatistics.Type.AudioMixers);
    }
}
