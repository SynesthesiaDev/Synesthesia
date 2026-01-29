using Common.Util;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Animations;

public class AnimationSequence : IAnimation
{
    public readonly List<IAnimation> Animations = [];

    public AnimationState State { get; set; } = AnimationState.Ready;

    public bool IsCompleted => State == AnimationState.Finished;

    public Action? OnComplete { get; set; } = null;

    public bool IsPaused { get; set; } = false;

    public long PausedTime { get; set; } = 0L;

    public bool Loop { get; set; } = false;

    private int currentIndex = 0;

    public IAnimation CurrentAnimation => Animations[currentIndex];

    public AnimationSequence(params IAnimation[] values)
    {
        foreach (var animation in values)
        {
            Animations.Add(animation);
        }
    }

    public AnimationSequence(List<IAnimation> values)
    {
        Animations.AddAll(values);
    }

    public void Start(long currentTime)
    {
        if (Animations.IsEmpty())
        {
            State = AnimationState.Finished;
            return;
        }

        State = AnimationState.Playing;
        CurrentAnimation.Start(currentTime);
    }

    public void Update(long currentTime)
    {
        if (IsCompleted || Animations.IsEmpty()) return;

        var current = CurrentAnimation;
        current.Update(currentTime);

        if (current.IsCompleted)
        {
            currentIndex++;
            if (currentIndex < Animations.Count)
            {
                Animations[currentIndex].Start(currentTime);
            }
            else
            {
                State = AnimationState.Finished;
            }
        }
    }

    public void Stop()
    {
        if (currentIndex < Animations.Count)
        {
            Animations[currentIndex].Stop();
        }
    }

    public void Reset()
    {
        State = AnimationState.Ready;
        currentIndex = 0;
        Animations.ForEach(anim =>
        {
            anim.Reset();
        });
    }

    public void Dispose()
    {
        Stop();
        Animations.ForEach(anim =>
        {
            anim.Dispose();
        });
        Animations.Clear();
    }

    public class Builder
    {
        private readonly List<IAnimation> animations = [];
        private bool isLooping = false;
        private Action? then = null;

        public Builder Add(IAnimation animation)
        {
            animations.Add(animation);
            return this;
        }

        public Builder Delay(long time)
        {
            animations.Add(new AnimationDelay(time));
            return this;
        }

        public Builder Then(Action action)
        {
            then = action;
            return this;
        }

        public Builder IsLooping(bool isLooping)
        {
            this.isLooping = isLooping;
            return this;
        }

        public AnimationSequence Build()
        {
            return new AnimationSequence(animations)
            {
                Loop = isLooping,
                OnComplete = then
            };
        }
    }
}
