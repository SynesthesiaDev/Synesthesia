using Synesthesia.Engine.Graphics.Two;

namespace Synesthesia.Engine.Input;

public interface IAcceptsFocus
{
    public Drawable2d GetOwningDrawable();
    
    public void OnFocusGained();

    public void OnFocusLost();

    public virtual void OnCharacterTyped(char character)
    {
        
    }
}