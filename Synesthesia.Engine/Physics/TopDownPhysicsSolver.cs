// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Physics;

public class TopDownPhysicsSolver : IPhysicsSolver2D
{
    public static readonly TopDownPhysicsSolver INSTANCE = new TopDownPhysicsSolver();

    public void Step(IHasPhysics2D physicsObject, Drawable2D drawable, FrameInfo frameInfo)
    {
        var deltaSeconds = frameInfo.DeltaSeconds;
        physicsObject.Velocity = ApplyDrag(physicsObject.Velocity, physicsObject.Drag * deltaSeconds);
        physicsObject.Velocity = Vector2.Clamp(physicsObject.Velocity, -physicsObject.MaxVelocity, physicsObject.MaxVelocity);
        drawable.X += physicsObject.Velocity.X * deltaSeconds;
        drawable.Y += physicsObject.Velocity.Y * deltaSeconds;
    }

    public static Vector2 ApplyDrag(Vector2 velocity, Vector2 drag)
    {
        return new Vector2
        {
            X = applyDragAxis(velocity.X, drag.X),
            Y = applyDragAxis(velocity.Y, drag.Y),
        };
    }

    private static float applyDragAxis(float velocity, float drag)
    {
        return velocity switch
        {
            > 0 => Math.Max(0, velocity - drag),
            < 0 => Math.Min(0, velocity + drag),
            _ => 0f,
        };
    }
}
