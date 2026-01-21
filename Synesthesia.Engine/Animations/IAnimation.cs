namespace Synesthesia.Engine.Animations;

public interface IAnimation : IDisposable
{
    public bool IsCompleted { get; set; }

    public bool Update(long currentTime);

    public bool Loop { get; set; }

    public void Stop();
    
    public void Reset();
    
    public void Start(long currentTime);
    
    public Action? OnComplete { get; set; }
    
    public bool IsPaused { get; set; }
    
    public long PausedTime { get; set; }

    public void MakeLooping()
    {
        Loop = true;
    }
    
}