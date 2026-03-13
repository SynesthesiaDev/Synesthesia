// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Util.Pooling;

namespace Synesthesia.Engine.Input.Events;

public class MouseMoveInputEvent : IPositionalInputEvent
{
    public void Reset()
    {
        Position = Vector2.Zero;
        PositionDelta = Vector2.Zero;
        Timestamp = 0;
    }

    public Vector2 Position { get; set; } = Vector2.Zero;

    public Vector2 PositionDelta { get; set; } = Vector2.Zero;

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    public InputEventSource Source => InputEventSource.Mouse;

    public long Timestamp { get; set; }

    public bool IsDown => false;

    public static MouseMoveInputEvent Rent() => InputHandler.MOUSE_MOVE_EVENT_POOL.Rent();

    public void ReturnToPool() => ReturnAction?.Invoke(this);

    public override string ToString()
    {
        return $"MouseMoveInputEvent(Position={Position},PositionDelta={PositionDelta},IsPooled={IsPooled},Timestamp={Timestamp})";
    }
}
