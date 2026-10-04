// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;

namespace Synesthesia.Engine.Physics;

public interface IHasPhysics2D
{
    Vector2 Velocity { get; set; }
    Vector2 MaxVelocity { get; set; }
    Vector2 Drag { get; set; }
}
