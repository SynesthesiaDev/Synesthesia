// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Physics;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Demo.TopDownPhysics;

public class SideScrollerTest : CompositeDrawable2D
{
    private LittlePhysicsGuy guy = null!;
    private const float velocity_modifier = 900.0f;

    [Singleton]
    private Game game = null!;

    protected override void OnLoading()
    {
        Children =
        [
            new PhysicsAndCollisionContainer2D
            {
                RelativeSizeAxes = Axes.Both,
                CollisionSolver = TestAABBSolver.INSTANCE,
                PhysicsSolver = new SideScrollingPhysicsSolver
                {
                    Gravity = new Vector2(0, 1600f)
                },
                Children =
                [
                    new FloorDrawable
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(500, 30),
                        Y = 400f
                    },
                    guy = new LittlePhysicsGuy(Color.FromHex("#ff85c0"))
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(32)
                    },
                    new LittlePhysicsGuy
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        X = 100,
                        Size = new Vector2(32)
                    },
                    new LittlePhysicsGuy
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        X = -100,
                        Size = new Vector2(32)
                    }
                ]
            }
        ];
    }

    protected override void OnUpdate(FrameInfo frameInfo)
    {
        var deltaSeconds = frameInfo.DeltaSeconds;

        if (Key.A.IsDown()) guy.Velocity -= new Vector2(velocity_modifier * deltaSeconds, 0f);
        if (Key.D.IsDown()) guy.Velocity += new Vector2(velocity_modifier * deltaSeconds, 0f);
        // if (Key.W.IsDown()) guy.Velocity -= new Vector2(0f, velocity_modifier * deltaSeconds);
        // if (Key.S.IsDown()) guy.Velocity += new Vector2(0f, velocity_modifier * deltaSeconds);

        if (Key.Space.IsDown()) guy.Velocity = guy.Velocity with { Y = -velocity_modifier };

        base.OnUpdate(frameInfo);
    }
}
