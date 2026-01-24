namespace Synesthesia.Engine.Animations;

public class AnimationDelay(long duration) : IAnimation
{
    public bool IsCompleted { get; set; }
    public bool Loop { get; set; } = false;

    private long startTime = -1;

    public void Start(long currentTime) => startTime = currentTime;

    public Action? OnComplete { get; set; } = null;

    public bool IsPaused { get; set; } = false;

    public long PausedTime { get; set; } = 0L;

    public bool Update(long currentTime)
    {
        if (startTime == -1 || IsCompleted) return IsCompleted;

        if (currentTime - startTime >= duration)
        {
            IsCompleted = true;
        }

        return IsCompleted;
    }

    public void Reset()
    {
        startTime = -1;
        IsCompleted = false;
    }

    public void Stop() => IsCompleted = true;

    public void Dispose()
    {
    }
}