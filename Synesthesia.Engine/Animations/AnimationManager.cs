using Synesthesia.Engine.Timing.Scheduling;

namespace Synesthesia.Engine.Animations;

public class AnimationManager : IDisposable
{
    private readonly object @lock = new();
    private long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

    private Dictionary<string, IAnimationHolder> keyedAnimations = [];
    private List<IAnimationHolder> animations = [];

    private Scheduler scheduler;

    public AnimationManager(Scheduler scheduler)
    {
        this.scheduler = scheduler;
        scheduler.Repeating(1, _ =>
        {
            currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            lock (@lock)
            {
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
                        animations.Remove(holder);
                        if (holder is ManagedAnimationHolder managed)
                        {
                            keyedAnimations.Remove(managed.Key);
                        }
                    }
                }
            }
        });
    }

    public void Restart(IAnimation animation)
    {
        animation.Reset();
        animation.Start(currentTime);
    }


    public void AddAnimation(IAnimation animation)
    {
        lock (@lock)
        {
            animations.Add(new UnmanagedAnimationHolder(animation));
            animation.Start(currentTime);
        }
    }

    public void AddAnimation(string field, IAnimation animation)
    {
        lock (@lock)
        {
            if (keyedAnimations.TryGetValue(field, out var existingHolder))
            {
                existingHolder.Animation.Stop();
                existingHolder.Animation.Dispose();
                keyedAnimations.Remove(field);
                animations.Remove(existingHolder);
            }

            var managed = new ManagedAnimationHolder(field, animation);
            keyedAnimations.Add(field, managed);
            animations.Add(managed);
            animation.Start(currentTime);
        }
    }

    public void Clear()
    {
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
            animations.Clear();
            keyedAnimations.Clear();
        }
    }

    public void Dispose()
    {
        Clear();
    }

    private interface IAnimationHolder
    {
        public IAnimation Animation { get; }
    }

    private record UnmanagedAnimationHolder(IAnimation Animation) : IAnimationHolder;

    private record ManagedAnimationHolder(string Key, IAnimation Animation) : IAnimationHolder;
}