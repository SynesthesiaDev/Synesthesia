using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util.Statistics;
using Synesthesia.Utils.Extensions;

namespace Synesthesia.Engine.Animations;

public class Animator : IDisposable
{
    protected internal bool IsDisposed { get; private set; }

    private readonly Lock animationLock = new();

    private readonly Dictionary<string, IAnimationHolder> keyedAnimations = [];
    private readonly List<IAnimationHolder> animations = [];

    public Animator() => EngineStatistics.Increment(EngineStatistics.Type.Animators);

    public void Update(FrameInfo frameInfo)
    {
        lock (animationLock)
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
                        holder.Animation.Start(frameInfo.TimeLong);
                        holder.Animation.State = AnimationState.Playing;
                        holder.Animation.Update(frameInfo.TimeLong);
                    }
                    else if (anim.State == AnimationState.Playing)
                    {
                        anim.Update(frameInfo.TimeLong);
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
                            anim.Update(frameInfo.TimeLong);
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

        lock (animationLock)
        {
            addAnimation(new UnmanagedAnimationHolder(animation));

            animation.State = AnimationState.Ready;
        }
    }

    public void AddAnimation(string field, IAnimation animation)
    {
        if (IsDisposed) return;

        lock (animationLock)
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
        lock (animationLock)
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
            EngineStatistics.Decrement(EngineStatistics.Type.Animations);
        }
    }

    private void addAnimation(IAnimationHolder animation)
    {
        lock (animationLock)
        {
            animations.Add(animation);
            animation.Animation.State = AnimationState.Ready;
            EngineStatistics.Increment(EngineStatistics.Type.Animations);
        }
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        lock (animationLock)
        {
            foreach (var holder in animations)
            {
                holder.Animation.Stop();
                if (holder is ManagedAnimationHolder managed)
                {
                    keyedAnimations.Remove(managed.Key);
                }
            }

            EngineStatistics.Set(EngineStatistics.Type.Animations, EngineStatistics.Get(EngineStatistics.Type.Animations) - animations.Count);
            animations.Clear();
            keyedAnimations.Clear();
        }
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        Clear();
        EngineStatistics.Decrement(EngineStatistics.Type.Animators);
        IsDisposed = true;
    }

    private interface IAnimationHolder
    {
        IAnimation Animation { get; }
    }

    private record UnmanagedAnimationHolder(IAnimation Animation) : IAnimationHolder;

    private record ManagedAnimationHolder(string Key, IAnimation Animation) : IAnimationHolder;
}
