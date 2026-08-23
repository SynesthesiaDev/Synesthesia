// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using Synesthesia.Utils;

namespace Synesthesia.Engine.Timing;

public class ThreadRunningClock : FramedClock, IDisposable
{
    private const double spin_threshold = 1.0;

    private readonly INativeSleep? nativeSleep = RuntimeInfo.OperatingSystem == RuntimeInfo.Platform.Windows ? new WindowsSleep() : new UnixSleep();

    public double MaximumUpdateHz
    {
        get;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Must be >= 0");

            field = value;
        }
    } = 1000;

    private double timeUntilNextFrame;

    public override void ProcessFrame()
    {
        base.ProcessFrame();

        if (MaximumUpdateHz is <= 0 or >= double.MaxValue) return;

        if (timeUntilNextFrame == 0 && ElapsedFrameTime == 0)
        {
            timeUntilNextFrame = 1000.0 / MaximumUpdateHz;
            return;
        }

        throttle();

    }

    private void throttle()
    {
        timeUntilNextFrame -= ElapsedFrameTime;

        if (timeUntilNextFrame > 0)
        {
            sleep(timeUntilNextFrame);
        }

        double targetFrameTime = 1000.0 / MaximumUpdateHz;
        timeUntilNextFrame += targetFrameTime;
    }

    private void sleep(double milliseconds)
    {
        if (milliseconds <= 0) return;

        var sleepStart = Stopwatch.GetTimestamp();

        if (milliseconds > spin_threshold)
        {
            var sleepTime = TimeSpan.FromMilliseconds(milliseconds - spin_threshold);
            if (nativeSleep?.Sleep(sleepTime) != true)
            {
                Thread.Sleep(sleepTime);
            }
        }

        while (true)
        {
            var elapsed = (Stopwatch.GetTimestamp() - sleepStart) * 1000.0 / Stopwatch.Frequency;
            if (elapsed >= milliseconds)
                break;

            Thread.SpinWait(10);
        }
    }

    public void Dispose()
    {
        nativeSleep?.Dispose();
    }
}
