using Synesthesia.Engine.Timing.Scheduling;

namespace Synesthesia.Engine.Animations;

public class AnimationManager : IDisposable
{
    private readonly object _lock = new();
    private long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

    private Dictionary<string, IAnimationHolder> KeyedAnimations = [];
    private List<IAnimationHolder> Animations = [];

    private Scheduler scheduler;

    public AnimationManager(Scheduler scheduler)
    {
        this.scheduler = scheduler;
        scheduler.Repeating(1, _ =>
        {
            currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            lock (_lock)
            {
                foreach (var holder in Animations.ToList())
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
                        Animations.Remove(holder);
                        if (holder is ManagedAnimationHolder managed)
                        {
                            KeyedAnimations.Remove(managed.Key);
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
        lock (_lock)
        {
            Animations.Add(new UnmanagedAnimationHolder(animation));
            animation.Start(currentTime);
        }
    }

    public void AddAnimation(string field, IAnimation animation)
    {
        lock (_lock)
        {
            if (KeyedAnimations.TryGetValue(field, out var existingHolder))
            {
                existingHolder.Animation.Stop();
                existingHolder.Animation.Dispose();
                KeyedAnimations.Remove(field);
                Animations.Remove(existingHolder);
            }

            var managed = new ManagedAnimationHolder(field, animation);
            KeyedAnimations.Add(field, managed);
            Animations.Add(managed);
            animation.Start(currentTime);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            foreach (var holder in Animations)
            {
                holder.Animation.Stop();
                if (holder is ManagedAnimationHolder managed)
                {
                    KeyedAnimations.Remove(managed.Key);
                }
            }
            Animations.Clear();
            KeyedAnimations.Clear();
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