using Raylib_cs;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Utility;

public static class KeyExtensions
{
    public static bool IsDown(this KeyboardKey keyboardKey)
    {
        return InputManager.IsDown(keyboardKey);
    }

    public static bool IsDown(this MouseButton mouseButton)
    {
        return InputManager.IsDown(mouseButton);
    }
}