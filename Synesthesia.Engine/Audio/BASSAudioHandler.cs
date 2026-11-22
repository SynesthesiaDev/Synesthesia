// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;
using ManagedBass;

namespace Synesthesia.Engine.Audio;

public abstract class BassAudioHandler : IAudioControl
{
    private int levelDspHandle;

    public abstract float Volume { get; set; }

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    //NOTICE: must stay here to keep the delegate alive!!!!!!!!!
    private DSPProcedure levelMeterDsp = null!;

    protected void AttachDspHandle(int channelMixdownHandle)
    {
        levelMeterDsp = levelMeterCallback;
        levelDspHandle = Bass.ChannelSetDSP(channelMixdownHandle, levelMeterDsp, IntPtr.Zero);
    }

    public AudioLevelBuffer Peak = new(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

    protected void DisposeDspHandle(int channelMixdownHandle)
    {
        if (channelMixdownHandle == 0 || levelDspHandle == 0) return;

        Bass.ChannelRemoveDSP(channelMixdownHandle, levelDspHandle);
        levelDspHandle = 0;
    }

    private void levelMeterCallback(int handle, int channel, IntPtr buffer, int length, IntPtr user)
    {
        unsafe
        {
            var sampleCount = length / sizeof(float);
            if (sampleCount <= 0) return;

            var samples = MemoryMarshal.Cast<byte, float>(new Span<byte>((void*)buffer, length));

            float maxL = 0f, maxR = 0f;

            // Audio is two 32-bit ints interleaved
            for (int i = 0; i + 1 < samples.Length; i += 2)
            {
                var l = MathF.Abs(samples[i]);
                var r = MathF.Abs(samples[i + 1]);
                if (l > maxL) maxL = l;
                if (r > maxR) maxR = r;
            }

            Peak = new AudioLevelBuffer(maxL * Volume, maxR * Volume, (maxL + maxR) / 2f * Volume);
        }
    }

    public void Dispose() { }
}
