// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Util;
using ManagedBass;
using Synesthesia.Engine.Audio.Controls;

namespace Synesthesia.Engine.Audio.Effect;

public abstract class AudioEffect<T> : IAudioEffect where T : IEffectParameter
{
    // public const int MAX_LOWPASS_CUTOFF = 22049; // nyquist - 1hz

    protected abstract EffectType GetEffectType();

    protected abstract int GetEffectPriority();

    protected int EffectHandle = 0;
    protected T Parameters;

    public bool IsActive => EffectHandle != 0 && AttachedAudioMixer != null;

    protected AudioMixer? AttachedAudioMixer = null;

    public int GetAudioHandle() => EffectHandle;

    public void AttachTo(IHasAudioHandle audioHandle)
    {
        if (IsActive) throw new InvalidOperationException($"{this.ObjectName()} is already attached to mixer {AttachedAudioMixer!.Identifier}. Use `Detach()` first");

        if (audioHandle is not AudioMixer audioMixer) throw new InvalidOperationException("Only audio mixer can have effects");
        if (audioHandle.GetAudioHandle() == 0) throw new ArgumentOutOfRangeException(nameof(audioHandle));

        EffectType type = GetEffectType();
        EffectHandle = Bass.ChannelSetFX(audioHandle.GetAudioHandle(), type, GetEffectPriority());

        if (EffectHandle == 0)
        {
            throw new InvalidOperationException($"Failed to add effect '{this.ObjectName()}' to mixer '{audioMixer.Identifier}': {Bass.LastError}");
        }

        AttachedAudioMixer = audioMixer;
        ApplyToStream();
    }

    public void Detach()
    {
        if (!IsActive) return;

        Bass.ChannelRemoveFX(EffectHandle, AttachedAudioMixer!.GetAudioHandle());
        EffectHandle = 0;
        AttachedAudioMixer = null;
    }

    public void ApplyToStream()
    {
        if (IsActive)
        {
            Bass.FXSetParameters(EffectHandle, Parameters);
        }
    }
}
