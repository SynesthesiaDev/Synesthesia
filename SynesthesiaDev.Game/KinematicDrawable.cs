// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Three;
using SynesthesiaUtil.Extensions;

namespace SynesthesiaDev.Game;

public abstract class KinematicDrawable : CompositeDrawable3d
{
    public Vector3 Velocity { get; set; } = Vector3.Zero;

    public bool HandleVelocityUpdates { get; set; } = true;

    public const float GRAVITY = 10f;

    public bool IsGrounded { get; set; } = false;

    //TODO Real collision
    public float GroundY { get; set; } = 0f;

    /// <summary>
    /// This is a tolerance for landing on the ground to prevent that
    /// tiny "flicker/tp" that some old games used to have
    /// </summary>
    public float GroundSnap { get; set; } = 0.02f;

    public float GravityScale { get; set; } = 1f;

    protected override void OnUpdate(FrameInfo frameInfo)
    {
        var delta = frameInfo.Delta.ToFloat();

        if (delta > 1f)
            delta /= 1000f;

        delta = MathF.Min(delta, 0.05f);

        if (HandleVelocityUpdates)
        {
            if (!IsGrounded)
            {
                Velocity = Velocity with { Y = Velocity.Y - (GRAVITY * GravityScale * delta) };
            } else if (Velocity.Y < 0f)
            {
                Velocity = Velocity with { Y = 0f };
            }
        }

        Position += Velocity * delta;

        resolveGroundPlane();

        base.OnUpdate(frameInfo);
    }

    private void resolveGroundPlane()
    {
        var eligibleToGround = Velocity.Y <= 0f;

        if (eligibleToGround && Position.Y <= GroundY + GroundSnap)
        {
            Position = Position with { Y = GroundY };
            IsGrounded = true;

            if (Velocity.Y < 0f)
                Velocity = Velocity with { Y = 0f };
        }
        else
        {
            IsGrounded = false;
        }
    }
}
