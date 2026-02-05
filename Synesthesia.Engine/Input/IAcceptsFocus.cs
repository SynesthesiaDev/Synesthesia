using Synesthesia.Engine.Graphics.Two;

namespace Synesthesia.Engine.Input;

public interface IAcceptsFocus
{
    Drawable2d GetOwningDrawable();

    void OnFocusGained();

    void OnFocusLost();

    void OnCharacterTyped(char character)
    {
    }
}
