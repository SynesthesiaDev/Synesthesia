using System.Diagnostics;

namespace Synesthesia.Engine.Timing;

public class StopwatchClock : Stopwatch, IAdjustableClock
{
    private double seekOffset = 0;

    private double rateChangeUsed = 0;

    private double rateChangeAccumulated = 0;

    private double stopwatchMilliseconds => (double)ElapsedTicks / Frequency * 1000;

    private double stopwatchCurrentTime => (stopwatchMilliseconds - rateChangeUsed) * rate + rateChangeAccumulated;

    public virtual double CurrentTime => stopwatchCurrentTime + seekOffset;

    private double rate = 1;

    public double Rate
    {
        get => rate;
        set
        {
            if (rate == value) return;

            rateChangeAccumulated += (stopwatchMilliseconds - rateChangeUsed) * rate;
            rateChangeUsed = stopwatchMilliseconds;

            rate = value;
        }
    }

    public StopwatchClock(bool start)
    {
        if (start) Start();
    }

    public new void Reset()
    {
        resetAccumulatedRate();
        base.Reset();
    }

    public new void Restart()
    {
        resetAccumulatedRate();
        base.Restart();
    }

    public bool Seek(double position)
    {
        seekOffset = position - stopwatchCurrentTime;
        return true;
    }

    public void ResetSpeedAdjustments() => Rate = 1;

    private void resetAccumulatedRate()
    {
        rateChangeAccumulated = 0;
        rateChangeUsed = 0;
    }
}