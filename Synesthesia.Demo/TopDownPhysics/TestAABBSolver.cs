// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Physics;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Demo.TopDownPhysics;

public class TestAABBSolver : ICollisionSolver2D
{
    public static readonly TestAABBSolver INSTANCE = new();

    public void Step(Span<Drawable2D> children, FrameInfo frameInfo)
    {
        for (var i = 0; i < children.Length; i++)
        {
            if (children[i] is not IHasCollision2D { CanBeCollidedWith: true } a)
                continue;

            var aBounds = toWorldSpace(a, children[i]);

            for (var j = i + 1; j < children.Length; j++)
            {
                if (children[j] is not IHasCollision2D { CanBeCollidedWith: true } b)
                    continue;

                var bBounds = toWorldSpace(b, children[j]);

                if (!aBounds.Intersects(bBounds))
                    continue;

                var resultA = a.OnCollision(b);
                var resultB = b.OnCollision(a);

                if (resultA == CollisionResult.Hit || resultB == CollisionResult.Hit)
                    resolveBlock(children[i], a, aBounds, children[j], b, bBounds);
            }
        }
    }

    private static void resolveBlock(Drawable2D drawableA, IHasCollision2D a, AABB2D aBounds,
        Drawable2D drawableB, IHasCollision2D b, AABB2D bBounds)
    {
        var overlapX = Math.Min(aBounds.Max.X, bBounds.Max.X) - Math.Max(aBounds.Min.X, bBounds.Min.X);
        var overlapY = Math.Min(aBounds.Max.Y, bBounds.Max.Y) - Math.Max(aBounds.Min.Y, bBounds.Min.Y);
        if (overlapX <= 0 || overlapY <= 0) return;

        var aCenter = (aBounds.Min + aBounds.Max) / 2f;
        var bCenter = (bBounds.Min + bBounds.Max) / 2f;

        Vector2 pushA;
        if (overlapX < overlapY)
            pushA = new Vector2(aCenter.X < bCenter.X ? -overlapX : overlapX, 0);
        else
            pushA = new Vector2(0, aCenter.Y < bCenter.Y ? -overlapY : overlapY);

        var aMovable = a is IHasPhysics2D physicsA;
        var bMovable = b is IHasPhysics2D physicsB2;

        if (aMovable && bMovable)
        {
            drawableA.X += pushA.X / 2f; drawableA.Y += pushA.Y / 2f;
            drawableB.X -= pushA.X / 2f; drawableB.Y -= pushA.Y / 2f;
        }
        else if (aMovable)
        {
            drawableA.X += pushA.X; drawableA.Y += pushA.Y;
        }
        else if (bMovable)
        {
            drawableB.X -= pushA.X; drawableB.Y -= pushA.Y;
        }

        if (aMovable && pushA.Y != 0 && ((IHasPhysics2D)a).Velocity.Y * pushA.Y < 0)
            ((IHasPhysics2D)a).Velocity = ((IHasPhysics2D)a).Velocity with { Y = 0 };
        if (bMovable && pushA.Y != 0 && ((IHasPhysics2D)b).Velocity.Y * -pushA.Y < 0)
            ((IHasPhysics2D)b).Velocity = ((IHasPhysics2D)b).Velocity with { Y = 0 };
    }

    private static AABB2D toWorldSpace(IHasCollision2D collidable, Drawable2D drawable)
    {
        var position = new Vector2(drawable.X, drawable.Y);
        return new AABB2D(collidable.AABB2D.Min + position, collidable.AABB2D.Max + position);
    }
}

