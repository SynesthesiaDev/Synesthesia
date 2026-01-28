using Common.Statistics;
using Synesthesia.Engine.Timing.Scheduling;

namespace Synesthesia.Engine.Animations;

public class Animator : IDisposable
{
    protected internal bool IsDisposed { get; private set; }

    private readonly object @lock = new();
    private readonly object schedulerLock = new();

    private readonly Dictionary<string, IAnimationHolder> keyedAnimations = [];
    private readonly List<IAnimationHolder> animations = [];

    private static long currentTime => DateTimeOffset.Now.ToUnixTimeMilliseconds();

    private Scheduler.RepeatingTask? updateTask;
    private readonly Scheduler scheduler;

    public Animator(Scheduler scheduler)
    {
        this.scheduler = scheduler;
        EngineStatistics.ANIMATORS.Increment();
    }

    private void sleep()
    {
        if (updateTask == null) return;

        updateTask?.Dispose();
        updateTask = null;
        EngineStatistics.ACTIVE_ANIMATORS.Decrement();
    }

    private void wakeUp()
    {
        if (updateTask != null) return;

        lock (schedulerLock)
        {
            EngineStatistics.ACTIVE_ANIMATORS.Increment();

            updateTask = scheduler.Repeating(1, _ =>
            {
                lock (@lock)
                {
                    if (animations.Count == 0)
                    {
                        sleep();
                        return;
                    }

                    foreach (var holder in animations.ToList())
                    {
                        holder.Animation.Update(currentTime);

                        if (!holder.Animation.IsCompleted) continue;

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
                    }

                    if (animations.Count == 0)
                    {
                        sleep();
                    }
                }
            });
        }
    }

    public void Restart(IAnimation animation)
    {
        wakeUp();

        animation.Reset();
        wakeUp();

        animation.Start(currentTime);
    }

    public void AddAnimation(IAnimation animation)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        lock (@lock)
        {
            addAnimation(new UnmanagedAnimationHolder(animation));

            wakeUp();
            animation.Start(currentTime);
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

            wakeUp();
            animation.Start(currentTime);
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
        if(IsDisposed) return;

        if (updateTask != null)
        {
            sleep();
        }

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
