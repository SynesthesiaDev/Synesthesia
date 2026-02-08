// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Bindable;
using Raylib_cs;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Utility;
using SynesthesiaDev.Game.Entities;

namespace SynesthesiaDev.Game;

public class Player : Entity
{
    private const float camera_height = 2f;

    private const float max_ground_speed = 6.0f;
    private const float max_air_speed = 6.0f;

    private const float ground_acceleration = 400.0f;
    private const float air_acceleration = 4.0f;
    private const float ground_friction = 14.0f;

    private const float jump_height = 1.5f;
    private const float jump_coyote_time = 0.10f;
    private const float jump_buffer_time = 0.10f;

    private const float look_sensitivity = 0.12f;
    private const float max_look_pitch = 89f;
    private const bool invert_look_y = false;

    private float coyoteTimer;
    private float jumpBufferTimer;

    public override float Speed { get; set; } = 5f;

    public Camera3d Camera { get; private set; } = null!;

    private readonly BindableProxy bindableProxy = new();

    public override BindableDouble Health { get; } = new()
    {
        Min = 0.0,
        Max = 20.0,
        Default = 20.0
    };

    private Synesthesia.Engine.Game game => DependencyContainer.Get<Synesthesia.Engine.Game>();

    protected override void OnLoading()
    {
        Children =
        [
            Camera = new Camera3d()
            {
                Position = new Vector3(0f, camera_height, 0f)
            }
        ];

        base.OnLoading();
    }

    private float yawDeg;
    private float pitchDeg;

    //TODO Inverted y axis cause some people use that ????

    protected override void LoadComplete()
    {
        bindableProxy.Subscribe(InputManager.ON_MOUSE_MOVE_DELTA, delta =>
        {
            if (!game.CursorConsumed) return;

            yawDeg -= delta.X * look_sensitivity;

            var pitchDelta = delta.Y * look_sensitivity;
            pitchDeg -= invert_look_y ? pitchDelta : -pitchDelta;

            pitchDeg = Math.Clamp(pitchDeg, -max_look_pitch, max_look_pitch);

            Rotation = Rotation with { Y = yawDeg };

            Camera.Rotation = Camera.Rotation with { X = pitchDeg, Z = 0f };
        });

        base.LoadComplete();
    }

    protected override void OnUpdate(FrameInfo frameInfo)
    {
        var delta = frameInfo.DeltaSeconds;

        if (KeyboardKey.Space.IsDown() && IsGrounded)
        {
            IsGrounded = false;
            var jumpVelocity = MathF.Sqrt(2f * GRAVITY * jump_height);
            Velocity = Velocity with { Y = jumpVelocity };
            jumpBufferTimer = jump_buffer_time;
        }

        coyoteTimer = !IsGrounded ? MathF.Max(0f, coyoteTimer - delta) : jump_coyote_time;

        jumpBufferTimer = MathF.Max(0f, jumpBufferTimer - delta);

        var moveInput = Vector2.Zero;
        if (KeyboardKey.W.IsDown()) moveInput.Y += 1f;
        if (KeyboardKey.S.IsDown()) moveInput.Y -= 1f;
        if (KeyboardKey.D.IsDown()) moveInput.X -= 1f;
        if (KeyboardKey.A.IsDown()) moveInput.X += 1f;

        if (moveInput != Vector2.Zero) moveInput = Vector2.Normalize(moveInput);

        // make move input relative to a look direction
        var yawRad = MathF.PI / 180f * yawDeg;
        var forwardVector = new Vector3(MathF.Sin(yawRad), 0f, MathF.Cos(yawRad));
        var rightVector = new Vector3(forwardVector.Z, 0f, -forwardVector.X);

        var desiredXzVector = (rightVector * moveInput.X + forwardVector * moveInput.Y) * Speed;

        // normalize so when holding, for example, both X and Z, we don't go over the speed limit
        if (desiredXzVector.LengthSquared() > 0f)
        {
            desiredXzVector = Vector3.Normalize(desiredXzVector);
        }

        var maxSpeed = IsGrounded ? max_ground_speed : max_air_speed;
        var desiredVelocity = desiredXzVector * maxSpeed;

        var horizontalVelocity = Velocity with { Y = 0f };

        // ground friction
        if (IsGrounded && moveInput != Vector2.Zero)
        {
            var drop = ground_friction * delta;
            horizontalVelocity = moveTowards(horizontalVelocity, Vector3.Zero, drop * max_ground_speed);
        }

        var acceleration = IsGrounded ? ground_acceleration : air_acceleration;
        horizontalVelocity = moveTowards(horizontalVelocity, desiredVelocity, acceleration * delta * maxSpeed);

        Velocity = Velocity with { X = horizontalVelocity.X, Z = horizontalVelocity.Z };

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            jumpBufferTimer = 0;
            coyoteTimer = 0f;

            IsGrounded = false;

            var jumpVelocity = MathF.Sqrt(2F * GRAVITY * jump_height);
            Velocity = Velocity with { Y = jumpVelocity };
        }

        base.OnUpdate(frameInfo);
    }

    private static Vector3 moveTowards(Vector3 current, Vector3 target, float maxDelta)
    {
        var delta = target - current;
        var distSquared = delta.LengthSquared();

        if (distSquared <= maxDelta * maxDelta || distSquared == 0f)
            return target;

        return current + delta / MathF.Sqrt(distSquared) * maxDelta;
    }

    protected override void Dispose(bool isDisposing)
    {
        bindableProxy.Dispose();

        base.Dispose(isDisposing);
    }
}
