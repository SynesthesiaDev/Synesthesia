// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Audio;

public struct AudioLevelBuffer(float peakLeft, float peakRight, float peak)
{
    public float PeakLeft = peakLeft;
    public float PeakRight = peakRight;
    public float Peak = peak;
};
