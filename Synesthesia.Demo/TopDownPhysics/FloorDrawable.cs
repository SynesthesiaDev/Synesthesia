// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Physics;

namespace Synesthesia.Demo.TopDownPhysics;

public class FloorDrawable: CompositeDrawable2D, IHasCollision2D
{
    public AABB2D AABB2D { get; set; }
    public bool CanBeCollidedWith => true;

    protected override void OnLoading()
    {
        Children =
        [
            new Box2D
            {
                RelativeSizeAxes = Axes.Both,
                Color = Color.Green
            }
        ];
        base.OnLoading();
    }

    public CollisionResult OnCollision(IHasCollision2D other)
    {
        return CollisionResult.Hit;
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
}
