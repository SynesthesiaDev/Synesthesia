// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Util.Pooling;

namespace Synesthesia.Engine.Input.Events;

public class TouchInputEvent : IInputEvent
{
    public void Reset()
    {
        Timestamp = 0;
    }

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    public InputEventSource Source => InputEventSource.Mouse;

    public long Timestamp { get; set; }

    public bool IsDown { get; set; }

    public Vector2 Position { get; set; } = Vector2.Zero;

    public ulong FingerId { get; set; }

    public float Pressure { get; set; }

    public static TouchInputEvent Rent() => InputHandler.TOUCH_INPUT_EVENT_POOL.Rent();

    public void ReturnToPool() => ReturnAction?.Invoke(this);

    public override string ToString()
    {
        return $"TouchInputEvent(IsDown={IsDown},FingerId={FingerId},Pressure={Pressure},Position={Position},IsPooled={IsPooled},Timestamp={Timestamp})";
    }
}
