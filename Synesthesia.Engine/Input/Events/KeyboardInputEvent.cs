// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Util.Pooling;

namespace Synesthesia.Engine.Input.Events;

public class KeyboardInputEvent : IInputEvent
{
    public void Reset()
    {
        Timestamp = 0;
        IsDown = false;
        IsRepeating = false;
    }

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    public Key Key { get; set; } = Key.Unknown;

    public InputEventSource Source => InputEventSource.Keyboard;

    public long Timestamp { get; set; }

    public bool IsDown { get; set; }

    public bool IsRepeating { get; set; }

    public static KeyboardInputEvent Rent() => InputHandler.KEYBOARD_EVENT_POOL.Rent();

    public void ReturnToPool() => ReturnAction?.Invoke(this);

    public override string ToString()
    {
        return $"KeyboardInputEvent(IsDown={IsDown},Key={Key},IsRepeating={IsRepeating},IsPooled={IsPooled},Timestamp={Timestamp})";
    }
}
