using Common.Statistics;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Timing.Scheduling;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Animations;

public class Animator : IDisposable
{
    protected internal bool IsDisposed { get; private set; }

    private readonly object @lock = new();

    private readonly Dictionary<string, IAnimationHolder> keyedAnimations = [];
    private readonly List<IAnimationHolder> animations = [];

    public Animator(Scheduler scheduler)
    {
        EngineStatistics.ANIMATORS.Increment();
    }

    public void Update(FrameInfo frameInfo)
    {
        lock (@lock)
        {
            if (animations.IsEmpty()) return;

            foreach (var holder in animations.ToList())
            {
                switch (holder.Animation.State)
                {
                    case AnimationState.Ready:
                    {
                        holder.Animation.Start(frameInfo.Time);
                        holder.Animation.State = AnimationState.Playing;
                        holder.Animation.Update(frameInfo.Time);
                        break;
                    }

                    case AnimationState.Playing:
                    {
                        holder.Animation.Update(frameInfo.Time);
                        break;
                    }
                    case AnimationState.Paused:
                        continue;
                    case AnimationState.Finished:
                    {
                        holder.Animation.OnComplete?.Invoke();
                        if (holder.Animation.Loop)
                        {
                            Restart(holder.Animation);
                        }
                        else
                        {
                            removeAnimation(holder);
                            if (holder is ManagedAnimationHolder managed)
                            {
                                keyedAnimations.Remove(managed.Key);
                            }
                        }
                        break;
                    }
                }
            }
        }
    }

    public void Restart(IAnimation animation)
    {
        animation.Reset();
        animation.State = AnimationState.Ready;
    }

    public void AddAnimation(IAnimation animation)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        lock (@lock)
        {
            addAnimation(new UnmanagedAnimationHolder(animation));

            animation.State = AnimationState.Ready;
        }
    }

    public void AddAnimation(string field, IAnimation animation)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        lock (@lock)
        {
            if (keyedAnimations.TryGetValue(field, out var existingHolder))
            {
                existingHolder.Animation.Stop();
                existingHolder.Animation.Dispose();
                keyedAnimations.Remove(field);
                removeAnimation(existingHolder);
            }

            var managed = new ManagedAnimationHolder(field, animation);
            keyedAnimations.Add(field, managed);
            addAnimation(managed);

            animation.State = AnimationState.Ready;
        }
    }

    private void removeAnimation(IAnimationHolder animation)
    {
        lock (@lock)
        {
            if (animations.Remove(animation))
            {
                EngineStatistics.ANIMATIONS.Decrement();
            }

            ;
        }
    }

    private void addAnimation(IAnimationHolder animation)
    {
        lock (@lock)
        {
            animations.Add(animation);
            EngineStatistics.ANIMATIONS.Increment();
        }
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        lock (@lock)
        {
            foreach (var holder in animations)
            {
                holder.Animation.Stop();
                if (holder is ManagedAnimationHolder managed)
                {
                    keyedAnimations.Remove(managed.Key);
                }
            }

            EngineStatistics.ANIMATIONS.Update(current => current - animations.Count);
            animations.Clear();
            keyedAnimations.Clear();
        }
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        Clear();
        EngineStatistics.ANIMATORS.Decrement();
        IsDisposed = true;
    }

    private interface IAnimationHolder
    {
        public IAnimation Animation { get; }
    }

    private record UnmanagedAnimationHolder(IAnimation Animation) : IAnimationHolder;

    private record ManagedAnimationHolder(string Key, IAnimation Animation) : IAnimationHolder;
}
