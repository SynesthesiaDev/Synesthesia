using Synesthesia.Engine.Timing.Scheduling;

namespace Synesthesia.Engine.Animations;

public class AnimationManager : IDisposable
{
    private Scheduler scheduler = new Scheduler();
    private long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    private Dictionary<string, IAnimation> Animations = [];

    public AnimationManager()
    {
        scheduler.Repeating(1, task =>
        {
            currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            foreach (var (field, animation) in Animations.ToList())
            {
                animation.Update(currentTime);
                if (animation.IsCompleted)
                {
                    Animations.Remove(field);
                }
            }
        });
    }

    public void AddAnimation<T>(string field, Animation<T> animation)
    {
        if (Animations.TryGetValue(field, out var existingAnimation))
        {
            existingAnimation.Stop();
            existingAnimation.Dispose();
            Animations.Remove(field);
        }

        Animations.Add(field, animation);
        animation.Start(currentTime);
    }

    public void Clear()
    {
        foreach (var (_, animation) in Animations)
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