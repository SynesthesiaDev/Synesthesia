using Common.Logger;
using Common.Util;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Animations;

public class AnimationSequence : IAnimation
{
    public List<IAnimation> Animations = [];
    
    public bool IsCompleted { get; set; }

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
            IsCompleted = true;
            return;
        }

        IsCompleted = false;
        CurrentAnimation.Start(currentTime);
    }
    
    public bool Update(long currentTime)
    {
        if (IsCompleted || Animations.IsEmpty()) return IsCompleted;
        
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
                IsCompleted = true;
            }
        }

        return IsCompleted;
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
        IsCompleted = false;
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

        public Builder IsLooping(bool isLooping)
        {
            this.isLooping = isLooping;
            return this;
        }

        public AnimationSequence Build()
        {
            return new AnimationSequence(animations)
            {
                Loop = isLooping
            };
        }
    }
}