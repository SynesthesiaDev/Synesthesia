// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Util.Pooling;

namespace Synesthesia.Engine.Input.Events;

public class MouseScrollInputEvent : IInputEvent
{
    public void Reset()
    {
        Delta = 0f;
        Timestamp = 0;
    }

    public float Delta { get; set; }

    public bool IsPooled { get; set; }
    public Action<IPooledObject>? ReturnAction { get; set; }

    public InputEventSource Source => InputEventSource.Mouse;

    public long Timestamp { get; set; }

    public bool IsDown => false;

    public static MouseScrollInputEvent Rent() => InputHandler.MOUSE_SCROLL_INPUT_EVENT_POOL.Rent();

    public void ReturnToPool() => ReturnAction?.Invoke(this);

    public override string ToString()
    {
        return $"MouseScrollInputEvent(Delta={Delta},IsPooled={IsPooled},Timestamp={Timestamp})";
    }
}
