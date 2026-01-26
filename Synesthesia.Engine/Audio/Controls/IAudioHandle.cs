
namespace Synesthesia.Engine.Audio.Controls;

public interface IHasAudioHandle
{
    int GetAudioHandle();

    void AttachTo(IHasAudioHandle audioHandle);

}
