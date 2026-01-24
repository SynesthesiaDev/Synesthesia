namespace Synesthesia.Engine.Audio.Controls;

public interface IHasAudioHandle
{
    public int GetAudioHandle();
    
    public void AttachTo(IHasAudioHandle audioHandle);
}