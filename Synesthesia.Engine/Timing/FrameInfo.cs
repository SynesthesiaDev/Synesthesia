// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;
using Synesthesia.Engine.Threading;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Timing;

[StructLayout(LayoutKind.Auto)]
public readonly struct FrameInfo
{
    public required double Delta { get; init; }

    public required double Time { get; init; }

    public required long Timestamp { get; init; }

    public required  ThreadType Type { get; init; }

    public required  ulong FrameIndex { get; init; }

    public long TimeLong => (long)Time;

    public float DeltaSeconds
    {
        get
        {
            var delta = Delta.ToFloat();
            if (delta > 1f) delta /= 1000f;
            return MathF.Min(delta, 0.05f);
        }
    }
}
