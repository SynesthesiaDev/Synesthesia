namespace Synesthesia.Engine.Animations;

public class AnimationDelay(long duration) : IAnimation
{
    public AnimationState State { get; set; } = AnimationState.Ready;
    public bool IsCompleted => State == AnimationState.Finished;
    public bool Loop { get; set; } = false;

    private long startTime = -1;

    public void Start(long currentTime) => startTime = currentTime;

    public Action? OnComplete { get; set; } = null;

    public bool IsPaused => State == AnimationState.Paused;

    public long PausedTime { get; set; } = 0L;

    public void Update(long currentTime)
    {
        if (startTime == -1 || IsCompleted) return;

        if (currentTime - startTime >= duration)
        {
            State = AnimationState.Finished;
        }

        return;
    }

    public void Reset()
    {
        startTime = -1;
        State = AnimationState.Ready;
    }

    public void Stop() => State = AnimationState.Finished;

    public void Dispose()
    {
    }
}
