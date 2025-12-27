namespace Synesthesia.Engine.Animation;

public interface IAnimation
{
    public bool IsCompleted { get; set; }

    public bool Update(long currentTime);

    public void Stop();

}