namespace Synesthesia.Engine.Audio.Controls;

public interface IPlaybackAudioControl : IAudioControl
{
    public bool IsPaused { get; }
    
    public void Pause();
    
    public void Resume();

    public void Seek(double time);

    public void Seek(long bytes);

}