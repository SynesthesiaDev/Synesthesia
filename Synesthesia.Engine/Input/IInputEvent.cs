using Common.Pooling;
using Raylib_cs;
using Synesthesia.Engine.Input.Events;

namespace Synesthesia.Engine.Input;

public interface IInputEvent : IPooledObject
{
    InputSource Source { get; }
    float Timestamp { get; }

    bool IsDown { get; }

    bool IsPrimary()
    {
        return this switch
        {
            KeyInputEvent keyInputEvent => keyInputEvent.Key == KeyboardKey.Enter,
            MouseButtonInputEvent mouseInputEvent => mouseInputEvent.Button == MouseButton.Left,
            _ => false
        };
    }
}

public enum InputSource
{
    Keyboard,
    Mouse,
    Touch,
}
