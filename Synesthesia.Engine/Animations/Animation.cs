using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Animations;

public sealed class Animation<T> : IAnimation
{
    public required T StartValue { get; init; }
    public required T EndValue { get; init; }
    public required long Duration { get; init; }
    public required Transform<T> Transform { get; init; }
    public required Easing Easing { get; init; }
    public required Action<T> OnUpdate { get; init; }
    public required Action? OnComplete { get; set; }
    public required long Delay { get; init; }

    public long StartTime { get; private set; } = -1;
    public bool IsPaused => State == AnimationState.Paused;

    public long PausedTime { get; set; }

    public AnimationState State { get; set; } = AnimationState.Ready;
    public bool IsCompleted => State == AnimationState.Finished;

    public bool Loop { get; set; }

    private EasingFunction easingFunction => new(Easing);

    public bool IsRunning => StartTime != -1 && !IsCompleted && !IsPaused;

    public long CurrentTime;

    public void Start(long currentTime)
    {
        if (StartTime == -1)
        {
            CurrentTime = currentTime;
            StartTime = currentTime + Delay;
        }
    }

    public void Pause()
    {
        State = AnimationState.Paused;
    }

    public void Resume(long currentTime)
    {
        if (!IsPaused) return;

        CurrentTime = currentTime;
        StartTime = currentTime - PausedTime;
        State = AnimationState.Playing;
    }

    public void Stop()
    {
        State = AnimationState.Finished;
    }

    public void Reset()
    {
        StartTime = -1;
        PausedTime = 0;
        State = AnimationState.Ready;
    }

    public void Update(long currentTime)
    {
        if (StartTime == -1 || IsCompleted || IsPaused) return;
        if (currentTime < StartTime) return;
        CurrentTime = currentTime;

        var elapsed = currentTime - StartTime;
        if (elapsed >= Duration)
        {
            OnUpdate.Invoke(EndValue);
            State = AnimationState.Finished;
            return;
        }

        var progress = (float)elapsed / Duration;
        var easedProgress = easingFunction.ApplyEasing(progress).ToFloat();
        var currentValue = Transform.Apply(StartValue, EndValue, easedProgress);

        OnUpdate.Invoke(currentValue);
    }

    public void Dispose()
    {
        Stop();
    }

    public Animation<T> Then(Action then)
    {
        if (Loop) throw new InvalidOperationException("Cannot have looping and Then() on animation at the same time");
        OnComplete = then;
        return this;
    }

    public Animation<T> MakeLooping()
    {
        Loop = true;
        return this;
    }

    public Animation<T> ThenHide(Drawable2D drawable)
    {
        OnComplete = () => drawable.Visible = false;
        return this;
    }
}
