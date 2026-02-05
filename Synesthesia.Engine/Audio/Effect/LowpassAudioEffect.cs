// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Bindable;
using ManagedBass;
using ManagedBass.Fx;

namespace Synesthesia.Engine.Audio.Effect;

public class LowpassAudioEffect : AudioEffect<BQFParameters>, IDisposable
{
    public const int MAX_LOWPASS_CUTOFF_HZ = 17640;

    public readonly BindableFloat Cutoff;

    public LowpassAudioEffect(float initialCutoff = MAX_LOWPASS_CUTOFF_HZ)
    {
        Cutoff = new BindableFloat
        {
            Min = 128f,
            Max = MAX_LOWPASS_CUTOFF_HZ,
            Default = initialCutoff,
        };

        Parameters = new BQFParameters
        {
            lFilter = BQFType.LowPass,
            fQ = 5f,
            fCenter = initialCutoff
        };

        Cutoff.OnValueChange(e =>
        {
            Parameters.fCenter = e.NewValue;
            ApplyToStream();
        }, true);

        Cutoff.Value = initialCutoff;
    }

    protected override EffectType GetEffectType() => EffectType.BQF;

    protected override int GetEffectPriority() => 1;

    public void Dispose()
    {
        Cutoff.Dispose();
    }
}
