// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass;
using Synesthesia.Engine.Threading;

namespace Synesthesia.Engine.Audio;

public record DecodedPcm(float[] Data, int Frequency, int Channels, double Length) : IDisposable
{
    public int SampleHandle { get; private set; }

    public int EnsureSample()
    {
        if (SampleHandle != 0) return SampleHandle;

        SampleHandle = Bass.CreateSample(Data.Length * sizeof(float), Frequency, Channels, 32, BassFlags.Float | BassFlags.SampleOverrideLongestPlaying);

        if (SampleHandle == 0 || !Bass.SampleSetData(SampleHandle, Data))
            throw new InvalidOperationException($"Failed to create sample: {Bass.LastError}");

        return SampleHandle;
    }

    public static DecodedPcm Decode(byte[] encoded)
    {
        ThreadSafety.AssertRunningOnAudioThread();

        int decoder = Bass.CreateStream(encoded, 0, encoded.Length, BassFlags.Decode | BassFlags.Float);
        if (decoder == 0) throw new InvalidOperationException(Bass.LastError.ToString());

        try
        {
            Bass.ChannelGetInfo(decoder, out var info);
            long byteLength = Bass.ChannelGetLength(decoder);

            var pcm = new float[byteLength / sizeof(float)];
            int got = Bass.ChannelGetData(decoder, pcm, (int)byteLength);

            // some formats return less per call
            while (got > 0)
            {
                got = Bass.ChannelGetData(decoder, pcm, (int)byteLength);
            }

            double seconds = Bass.ChannelBytes2Seconds(decoder, byteLength);
            return new DecodedPcm(pcm, info.Frequency, info.Channels, seconds);
        }
        finally
        {
            Bass.StreamFree(decoder);
        }
    }

    public void Dispose()
    {
        if (SampleHandle != 0) Bass.SampleFree(SampleHandle);
    }
}
