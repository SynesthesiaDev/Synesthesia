using System.Numerics;
using Raylib_cs;

namespace Synesthesia.Engine.Input;

public interface IInputEvent
{
    InputSource Source { get; }
    float Timestamp { get; }

    bool IsDown { get; }

    public bool IsPrimary()
    {
        return this switch
        {
            KeyInputEvent keyInputEvent => keyInputEvent.Key == KeyboardKey.Enter,
            MouseButtonInputEvent mouseInputEvent => mouseInputEvent.Button == MouseButton.Left,
            TouchInputEvent touchInputEvent => touchInputEvent.Gesture == Gesture.Tap,
            _ => false
        };
    }
}



public record KeyInputEvent(KeyboardKey Key, bool IsDown, InputSource Source = InputSource.Keyboard) : IInputEvent
{
    public float Timestamp { get; } = (float)Raylib.GetTime();
}

public record MouseButtonInputEvent(MouseButton Button, bool IsDown) : IInputEvent
{
    public InputSource Source => InputSource.Mouse;
    public float Timestamp { get; } = (float)Raylib.GetTime();
}

public record MouseMoveInputEvent(Vector2 Position) : IInputEvent
{
    public InputSource Source => InputSource.Mouse;
    public float Timestamp => (float)Raylib.GetTime();
    public bool IsDown => false;
}

public record TouchInputEvent(Gesture Gesture, Vector2 Position, bool IsDown) : IInputEvent
{
    public InputSource Source => InputSource.Touch;
    public float Timestamp { get; } = (float)Raylib.GetTime();
}

public enum InputSource
{
    Keyboard,
    Mouse,
    // Controller,
    Touch
}