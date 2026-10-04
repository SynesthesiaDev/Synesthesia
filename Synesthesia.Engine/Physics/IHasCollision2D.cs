// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Physics;

public interface IHasCollision2D
{
    AABB2D AABB2D { get; set; }
    bool CanBeCollidedWith { get; }
    CollisionResult OnCollision(IHasCollision2D other);
}
