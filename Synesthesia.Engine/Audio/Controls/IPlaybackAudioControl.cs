namespace Synesthesia.Engine.Audio.Controls;

public interface IPlaybackAudioControl : IAudioControl
{
    bool IsPaused { get; }

    void Pause();

    void Resume();

    void Seek(double time);

    void Seek(long bytes);

}