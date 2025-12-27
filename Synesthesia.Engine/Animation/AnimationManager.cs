using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Timing.Scheduling;

namespace Synesthesia.Engine.Animation;

public class AnimationManager : IDisposable
{
    private Scheduler scheduler = new Scheduler();
    private long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    private List<IAnimation> Animations = [];

    public AnimationManager()
    {
        scheduler.Repeating(1, task =>
        {
            currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            foreach (var animation in Animations.ToList())
            {
                animation.Update(currentTime);
                if (animation.IsCompleted)
                {
                    Animations.Remove(animation);
                }
            }
        });
    }

    public void AddAnimation<T>(Animation<T> animation)
    {
        Animations.Add(animation);
        animation.Start(currentTime);
    }

    public void Clear()
    {
        foreach (var animation in Animations)
        {
            animation.Stop();
        }
        Animations.Clear();
    }


    public void Dispose()
    {
        Clear();
        scheduler.Dispose();
    }
}