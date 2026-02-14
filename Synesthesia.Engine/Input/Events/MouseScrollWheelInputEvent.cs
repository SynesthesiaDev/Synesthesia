// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Pooling;

namespace Synesthesia.Engine.Input.Events;

public class MouseScrollWheelInputEvent : IInputEvent
{
    public void Reset()
    {
        Delta = 0f;
        Timestamp = 0f;
    }

    public float Delta { get; set; } = 0f;

    public bool IsPooled { get; set; }
    public Action<IPooledObject>? ReturnAction { get; set; }

    public InputSource Source => InputSource.Mouse;

    public float Timestamp { get; set; } = 0f;

    public bool IsDown => false;
}
