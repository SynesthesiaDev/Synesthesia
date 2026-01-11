using System.Numerics;
using Raylib_cs;

namespace Synesthesia.Engine.Input;

public interface IInputEvent
{
    InputSource Source { get; }
    float Timestamp { get; }

    public bool IsPrimary()
    {
        return this switch
        {
            KeyInputEvent keyInputEvent => keyInputEvent.Key == KeyboardKey.Enter,
            MouseInputEvent mouseInputEvent => mouseInputEvent.Button == MouseButton.Left,
            TouchInputEvent touchInputEvent => touchInputEvent.Gesture == Gesture.Tap,
            _ => false
        };
    }
}

public record KeyInputEvent(KeyboardKey Key, bool IsDown, InputSource Source = InputSource.Keyboard) : IInputEvent 
{
    public float Timestamp { get; } = (float)Raylib.GetTime();
}

public record MouseInputEvent(MouseButton Button, Vector2 Position, bool IsDown) : IInputEvent
{
    public InputSource Source => InputSource.Mouse;
    public float Timestamp { get; } = (float)Raylib.GetTime();
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