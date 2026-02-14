// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Pooling;
using Raylib_cs;

namespace Synesthesia.Engine.Input.Events;

public class MouseButtonInputEvent : IInputEvent
{
    public void Reset()
    {
        Timestamp = 0f;
        IsDown = false;
        Button = MouseButton.Left;
    }

    public MouseButton Button { get; set; } = MouseButton.Left;

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    public InputSource Source => InputSource.Mouse;

    public float Timestamp { get; set; } = 0f;
    public bool IsDown { get; set; } = false;
}

// public record MouseButtonInputEvent(MouseButton Button, bool IsDown) : IInputEvent
// {
    // public InputSource Source => InputSource.Mouse;
    // public float Timestamp { get; } = (float)Raylib.GetTime();
// }
