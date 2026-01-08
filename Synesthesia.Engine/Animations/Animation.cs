using Synesthesia.Engine.Animations.Easings;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Animations;

public partial class Animation<T> : IAnimation
{

    public required T StartValue { get; init; }
    public required T EndValue { get; init; }
    public required long Duration { get; init; }
    public required Transform<T> Transform { get; init; }
    public required Easing Easing { get; init; }
    public required Action<T> OnUpdate { get; init; }
    public required Action<T>? OnComplete { get; set; }
    public required long Delay { get; init; }

    public long StartTime { get; private set; } = -1;
    public bool IsPaused { get; private set; } = false;
    public long PausedTime { get; private set; } = 0L;
    public bool IsCompleted { get; set; } = false;

    private EasingFunction easingFunction => new(Easing);

    public bool IsRunning => StartTime != -1 && !IsCompleted && !IsPaused;

    public void Start(long currentTime)
    {
        if (StartTime == -1)
        {
            StartTime = currentTime + Delay;
        }
    }

    public void Pause()
    {
        if (!IsPaused) IsPaused = true;
    }

    public void Resume(long currentTime)
    {
        if (!IsPaused) return;

        StartTime = currentTime - PausedTime;
        IsPaused = false;
    }

    public void Stop()
    {
        IsCompleted = true;
    }

    public void Reset()
    {
        StartTime = -1;
        IsCompleted = false;
        IsPaused = false;
        PausedTime = 0;
    }

    public bool Update(long currentTime)
    {
        if (StartTime == -1 || IsCompleted || IsPaused) return false;
        if (currentTime < StartTime) return false;

        var elapsed = currentTime - StartTime;
        if (elapsed >= Duration)
        {
            OnUpdate.Invoke(EndValue);
            IsCompleted = true;
            OnComplete?.Invoke(EndValue);
            return true;
        }

        var progress = (float)elapsed / Duration;
        var easedProgress = easingFunction.ApplyEasing(progress).ToFloat();
        var currentValue = Transform.Apply(StartValue, EndValue, easedProgress);

        OnUpdate.Invoke(currentValue);

        return false;
    }


    public void Dispose()
    {
        Stop();
    }

    public void Then(Action<T> then)
    {
        OnComplete = then;
    }
}