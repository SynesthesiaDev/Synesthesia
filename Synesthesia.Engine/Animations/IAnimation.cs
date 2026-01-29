namespace Synesthesia.Engine.Animations;

public interface IAnimation : IDisposable
{
    AnimationState State { get; set; }

    bool IsCompleted => State == AnimationState.Finished;

    void Update(long currentTime);

    bool Loop { get; set; }

    void Stop();

    void Reset();

    void Start(long currentTime);

    Action? OnComplete { get; set; }

    bool IsPaused => State == AnimationState.Paused;

    long PausedTime { get; set; }

    void MakeLooping()
    {
        Loop = true;
    }
}

