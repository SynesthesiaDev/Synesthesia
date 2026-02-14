// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Pooling;

namespace Synesthesia.Engine.Input.Events;

public class MouseMoveInputEvent : IInputEvent
{
    public void Reset()
    {
        Position = Vector2.Zero;
        PositionDelta = Vector2.Zero;
        Timestamp = 0f;
    }

    public Vector2 Position { get; set; } = Vector2.Zero;

    public Vector2 PositionDelta { get; set; } = Vector2.Zero;

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    public InputSource Source => InputSource.Mouse;

    public float Timestamp { get; set; } = 0f;

    public bool IsDown => false;
}


// public record MouseMoveInputEvent(Vector2 Position, Vector2 PositionDelta) : IInputEvent
// {
//     public InputSource Source => InputSource.Mouse;
//     public float Timestamp => (float)Raylib.GetTime();
//     public bool IsDown => false;
// }
