namespace Synesthesia.Engine.Timing;

public interface IAdjustableClock
{
    void Reset();

    void Start();

    void Stop();

    bool Seek(double position);

    double Rate { get; set; }

    void ResetSpeedAdjustments();
}