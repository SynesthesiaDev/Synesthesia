// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Util.Pooling;

namespace Synesthesia.Engine.Input.Events;

public class MouseButtonInputInputEvent : ICursorInputEvent
{
    public void Reset()
    {
        Timestamp = 0;
        IsDown = false;
        Button = MouseButton.Left;
    }

    public MouseButton Button { get; set; } = MouseButton.Left;

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    public InputEventSource Source => InputEventSource.Mouse;

    public long Timestamp { get; set; }

    public bool IsDown { get; set; }


    public bool IsPrimary { get; set; }

    public static MouseButtonInputInputEvent Rent() => InputHandler.MOUSE_BUTTON_EVENT_POOL.Rent();

    public void ReturnToPool() => ReturnAction?.Invoke(this);

    public override string ToString()
    {
        return $"MouseButtonInputEvent(IsDown={IsDown},Button={Button},IsPooled={IsPooled},Timestamp={Timestamp})";
    }
}

