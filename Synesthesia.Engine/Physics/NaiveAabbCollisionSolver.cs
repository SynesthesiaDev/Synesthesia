// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Physics;

public class NaiveAabbCollisionSolver : ICollisionSolver2D
{
    public static readonly NaiveAabbCollisionSolver INSTANCE = new();

    public void Step(Span<Drawable2D> children, FrameInfo frameInfo)
    {
        foreach (ref Drawable2D drawable in children)
        {
            if(drawable is not IHasCollision2D { CanBeCollidedWith: true } a)
                continue;

            var aBounds = toWorldSpace(a, drawable);

            foreach (ref Drawable2D child in children)
            {
                if(child is not IHasCollision2D { CanBeCollidedWith: true } b)
                    continue;

                var bBounds = toWorldSpace(b, child);

                if(!aBounds.Intersects(bBounds))
                    continue;

                a.OnCollision(b);
                b.OnCollision(a);
            }
        }
    }

    private static AABB2D toWorldSpace(IHasCollision2D collidable, Drawable2D drawable)
    {
        var position = new Vector2(drawable.X, drawable.Y);
        return new AABB2D(collidable.AABB2D.Min + position, collidable.AABB2D.Max + position);
    }
}
