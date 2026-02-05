namespace Synesthesia.Engine.Audio.Controls;

public interface IAudioControl : IDisposable
{
    float Volume { get; set; }
    
}