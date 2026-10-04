// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Physics;


public class SideScrollingPhysicsSolver : IPhysicsSolver2D
{
    public static readonly SideScrollingPhysicsSolver INSTANCE = new SideScrollingPhysicsSolver();

    public Vector2 Gravity { get; set; } = new Vector2(0, 9.81f);

    public float GroundY { get; set; }

    public bool EnableGroundCollision { get; set; } = false;

    public void Step(IHasPhysics2D physicsObject, Drawable2D drawable, FrameInfo frameInfo)
    {
        float delta = frameInfo.DeltaSeconds;

        physicsObject.Velocity += Gravity * delta;

        physicsObject.Velocity = ApplyDrag(physicsObject.Velocity, physicsObject.Drag * delta);

        physicsObject.Velocity = Vector2.Clamp(physicsObject.Velocity, -physicsObject.MaxVelocity, physicsObject.MaxVelocity);

        drawable.X += physicsObject.Velocity.X * delta;
        drawable.Y += physicsObject.Velocity.Y * delta;

        if (EnableGroundCollision)
        {
            float objectBottom = drawable.Y + drawable.Height;
            if (objectBottom > GroundY)
            {
                drawable.Y = GroundY - drawable.Height;
                if (physicsObject.Velocity.Y > 0)
                    physicsObject.Velocity = physicsObject.Velocity with { Y = 0f };
            }
        }
    }

    public static Vector2 ApplyDrag(Vector2 velocity, Vector2 drag)
    {
        return new Vector2
        {
            X = applyDragAxis(velocity.X, drag.X),
            Y = applyDragAxis(velocity.Y, drag.Y)
        };
    }

    private static float applyDragAxis(float velocity, float drag)
    {
        return velocity switch
        {
            > 0 => Math.Max(0, velocity - drag),
            < 0 => Math.Min(0, velocity + drag),
            _ => 0f
        };
    }
}
