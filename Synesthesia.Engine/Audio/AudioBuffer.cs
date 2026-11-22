// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Resources;

namespace Synesthesia.Engine.Audio;

public class AudioBuffer(string name, byte[] data, int frequency, int channels, double length)
    : INamedResource
{
    private static ulong nextId;

    public ulong StaticBatchId { get; } = Interlocked.Increment(ref nextId);

    public string ResourceName { get; } = name;

    public DecodedPcm PcmData
    {
        get
        {
            field ??= DecodedPcm.Decode(Data);
            return field;
        }
    } = null!;

    public readonly byte[] Data = data;

    public readonly int Frequency = frequency;

    public readonly int Channels = channels;

    public readonly double Length = length;

    public void Dispose()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (PcmData != null)
        {
            PcmData.Dispose();
        }
    }
}
