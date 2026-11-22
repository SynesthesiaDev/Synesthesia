// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Audio;

[StructLayout(LayoutKind.Auto)]
public readonly struct AudioLevelBuffer(float peakLeft, float peakRight, float peak)
{
    public readonly float PeakLeft = peakLeft;
    public readonly float PeakRight = peakRight;
    public readonly float Peak = peak;
};

