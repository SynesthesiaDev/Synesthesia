namespace Synesthesia.Engine.Animations;

public interface IAnimation : IDisposable
{
    public bool IsCompleted { get; set; }

    public bool Update(long currentTime);

    public void Stop();

}