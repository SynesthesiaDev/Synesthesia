// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Timing;

public class FrameStatistics
{
    private const int tracking_window_size = 60;

    private readonly Queue<double> frameTimes = new();

    private double averageFrameTime;

    public double FramesPerSecond => averageFrameTime > 0 ? 1000.0 / averageFrameTime : 0;

    public double AverageFrameTime => averageFrameTime;

    public void Add(double frameTime)
    {
        frameTimes.Enqueue(frameTime);
        if (frameTimes.Count > tracking_window_size) frameTimes.Dequeue();

        averageFrameTime = frameTimes.Average();
    }
}
