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
            var newAnimationsAdded = false;
            var initialCount = animations.Count;

            do
            {
                for (int index = animations.Count - 1; index >= 0; index--)
                {
                    if (index >= animations.Count) continue;

                    var holder = animations[index];
                    var anim = holder.Animation;

                    if (anim.State == AnimationState.Ready)
                    {
                        holder.Animation.Start(frameInfo.Time);
                        holder.Animation.State = AnimationState.Playing;
                        holder.Animation.Update(frameInfo.Time);
                    }
                    else if (anim.State == AnimationState.Playing)
                    {
                        anim.Update(frameInfo.Time);
                    }
                    else if (anim.State == AnimationState.Paused)
                    {
                        continue;
                    }

                    if (anim.State == AnimationState.Finished)
                    {
                        if (anim.Loop)
                        {
                            Restart(anim);
                            anim.Update(frameInfo.Time);
                        }
                        else
                        {
                            removeAnimation(holder, index);
                            if (holder is ManagedAnimationHolder managed)
                            {
                                keyedAnimations.Remove(managed.Key);
                            }
                            anim.OnComplete?.Invoke();
                        }

                        if (animations.Count > initialCount)
                        {
                            newAnimationsAdded = true;
                        }
                    }
                }
            } while (newAnimationsAdded);
        }
    }

    public void Restart(IAnimation animation)
    {
        animation.Reset();
        animation.State = AnimationState.Ready;
    }

    public void AddAnimation(IAnimation animation)
    {
        if (IsDisposed) return;

        lock (@lock)
        {
            addAnimation(new UnmanagedAnimationHolder(animation));

            animation.State = AnimationState.Ready;
        }
    }

    public void AddAnimation(string field, IAnimation animation)
    {
        if (IsDisposed) return;

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

    private void removeAnimation(IAnimationHolder animation, int? index = null)
    {
        bool removed;
        lock (@lock)
        {
            if (index == null)
            {
                removed = animations.Remove(animation);
            }
            else
            {
                animations.RemoveAt(index.Value);
                removed = true;
            }
        }

        if (removed)
        {
            EngineStatistics.ANIMATIONS.Decrement();
        }
    }

    private void addAnimation(IAnimationHolder animation)
    {
        lock (@lock)
        {
            animations.Add(animation);
            animation.Animation.State = AnimationState.Ready;
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
        IAnimation Animation { get; }
    }

    private record UnmanagedAnimationHolder(IAnimation Animation) : IAnimationHolder;

    private record ManagedAnimationHolder(string Key, IAnimation Animation) : IAnimationHolder;
}
