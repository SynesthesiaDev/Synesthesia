// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Resources;
using Synesthesia.Utils.Types;

namespace Synesthesia.Engine.Audio;

public record Sound(AudioBuffer Buffer) : INamedResource
{
    public double Length = Buffer.Length;

    public PlaybackMode PlaybackMode { get; set; } = PlaybackMode.Decoded;

    public double RestartPoint { get; set; } = 0.0;

    public ulong StaticBatchId { get; } = Buffer.StaticBatchId;

    public string ResourceName { get; } = Buffer.ResourceName;

    public virtual bool Looping { get; set; } = false;

    public virtual int? Bitrate => null;

    public FloatRange DefaultVolume { get; set; } = new(1f, 1f);

    public FloatRange DefaultPitch { get; set; } = new(1f, 1f);

    public void Dispose()
    {
    }

}
