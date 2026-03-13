// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;

namespace Synesthesia.Engine.Input.Events;

public interface IPositionalInputEvent : IInputEvent
{
    Vector2 Position { get; set; }

    Vector2 PositionDelta { get; set; }

}
