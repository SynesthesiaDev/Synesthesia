// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Physics;

namespace Synesthesia.Demo.TopDownPhysics;

public class LittlePhysicsGuy(Color? color = null) : CompositeDrawable2D, IHasPhysics2D, IHasCollision2D
{
    public Vector2 Velocity { get; set; }
    public Vector2 MaxVelocity { get; set; } = new Vector2(400f);
    public Vector2 Drag { get; set; } = new Vector2(600f);

    public AABB2D AABB2D { get; set; } = new AABB2D(Vector2.Zero, Vector2.Zero);

    public bool CanBeCollidedWith => true;

    protected override void OnLoading()
    {
        Children =
        [
            new Box2D
            {
                RelativeSizeAxes = Axes.Both,
                Color = color ?? Color.White
            }
        ];
        base.OnLoading();
    }

    protected override void OnLayout(Invalidation dirty)
    {
        if (dirty.HasEitherFlag(Invalidation.Geometry, Invalidation.Size))
        {
            var originOffset = GetAnchorOffset(Size, Origin);
            AABB2D = new AABB2D(-originOffset, Size - originOffset);
        }
        base.OnLayout(dirty);
    }

    public CollisionResult OnCollision(IHasCollision2D other)
    {
        return CollisionResult.Hit;
    }
}
