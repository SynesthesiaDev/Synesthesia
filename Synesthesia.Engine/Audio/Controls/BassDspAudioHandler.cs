// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;
using ManagedBass;

namespace Synesthesia.Engine.Audio.Controls;

public abstract class BassDspAudioHandler : IAudioControl
{
    private int levelDspHandle;

    public abstract float Volume { get; set; }

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    //NOTICE: must stay here to keep the delegate alive
    private DSPProcedure levelMeterDsp = null!;

    protected void AttachDspHandle(int channelMixdownHandle)
    {
        levelMeterDsp = levelMeterCallback;
        levelDspHandle = Bass.ChannelSetDSP(channelMixdownHandle, levelMeterDsp, IntPtr.Zero, 0);
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
        unsafe //I love working with low-level libraries!!!!!!
        {
            // 2 32-bit floats per frame cause sure why not !! so logical
            var sampleCount = length / sizeof(float);
            if (sampleCount <= 0) return;

            var samples = MemoryMarshal.Cast<byte, float>(new Span<byte>((void*)buffer, length));

            float maxL = 0f, maxR = 0f;

            // Interleaved LRLR audio bruh
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
