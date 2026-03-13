// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;

namespace Synesthesia.Engine.Timing;

/// <summary>
/// A clock that tracks frame statistics and elapsed time.
/// </summary>
public class FramedClock
{
    protected long LastFrameTime;
    protected long CurrentFrameTime;

    private bool isFirstFrame = true;

    private readonly long startTimestamp = Stopwatch.GetTimestamp();

    public double ElapsedFrameTime { get; protected set; }

    public double CurrentTime
    {
        get
        {
            var now = Stopwatch.GetTimestamp();
            return (now - startTimestamp) * 1000.0 / Stopwatch.Frequency;
        }
    }

    public virtual void ProcessFrame()
    {
        LastFrameTime = CurrentFrameTime;
        CurrentFrameTime = Stopwatch.GetTimestamp();


        if (isFirstFrame)
        {
            isFirstFrame = false;
            ElapsedFrameTime = 0;
            return;
        }

        ElapsedFrameTime = (CurrentFrameTime - LastFrameTime) * 1000.0 / Stopwatch.Frequency;
    }
}
