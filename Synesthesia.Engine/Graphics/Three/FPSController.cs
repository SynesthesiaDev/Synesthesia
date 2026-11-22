// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util;

namespace Synesthesia.Engine.Graphics.Three;

public class FpsController : CharacterBody3D
{
    private const float camera_lean_amount = 0.3f;
    private const float camera_lean_responsiveness = 100f;

    private const float max_speed = 6.0f;
    private const float ground_acceleration = 120.0f;
    private const float air_acceleration = 40.0f;
    private const float ground_friction = 10.0f;
    private const float stop_speed = 2.0f;

    private const float jump_height = 1.0f;
    private const float jump_coyote_time = 0.10f;
    private const float jump_buffer_time = 0.05f;
    private const float gravity = 20f;

    private const float look_sensitivity = 0.12f;
    private const float max_look_pitch = 89f;

    [Injected]
    private InputHandler inputHandler = null!;

    [Injected]
    private Game game = null!;

    public Camera Camera { get; set; } = null!;

    public float Speed { get; set; } = 1f;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private float currentTilt;

    protected override void OnLoading()
    {
        inputHandler.SetMouseCaptured(true);
        Children =
        [
            Camera = new Camera(),
        ];

        Camera.Position = new Vector3(0, 0.7f, 0);
        Renderer.Camera = Camera;
        base.OnLoading();
    }

    protected override void OnCharacterUpdate(FrameInfo frameInfo)
    {
        var dt = frameInfo.DeltaSeconds;

        var delta = InputHandler.ConsumeMouseDelta();

        var currentYaw = Rotation.Y - delta.X * look_sensitivity;
        Rotation = new Vector3(0, currentYaw, 0);

        var currentPitch = Camera.Rotation.X - delta.Y * look_sensitivity;
        currentPitch = Math.Clamp(currentPitch, -max_look_pitch, max_look_pitch);

        var q = RawRotation;
        var forward = Vector3.Transform(-Vector3.UnitZ, q);
        var right = Vector3.Transform(Vector3.UnitX, q);
        forward.Y = 0;
        forward = Vector3.Normalize(forward);
        right.Y = 0;
        right = Vector3.Normalize(right);

        var desiredDirection = Vector3.Zero;
        if (Key.W.IsDown()) desiredDirection += forward;
        if (Key.S.IsDown()) desiredDirection -= forward;
        if (Key.D.IsDown()) desiredDirection += right;
        if (Key.A.IsDown()) desiredDirection -= right;
        if (desiredDirection != Vector3.Zero) desiredDirection = Vector3.Normalize(desiredDirection);

        coyoteTimer = IsGrounded ? jump_coyote_time : MathF.Max(0f, coyoteTimer - dt);
        jumpBufferTimer = Key.Space.IsDown() ? jump_buffer_time : MathF.Max(0f, jumpBufferTimer - dt);

        var jumping = jumpBufferTimer > 0f && coyoteTimer > 0f;
        var onGround = IsGrounded && !jumping;

        var hasInput = desiredDirection != Vector3.Zero;
        var applyFriction = IsGrounded && (!jumping || !hasInput);

        var horizontalVel = Velocity with { Y = 0 };

        if (onGround)
        {
            var speed = horizontalVel.Length();
            if (speed > 0.001f)
            {
                var control = MathF.Max(speed, stop_speed);
                var drop = control * ground_friction * dt;
                horizontalVel *= MathF.Max(speed - drop, 0f) / speed;
            }
        }

        var maxSpeed = max_speed * Speed;
        var oldSpeed = horizontalVel.Length();

        horizontalVel = accelerate(horizontalVel, desiredDirection, maxSpeed,
            onGround ? ground_acceleration : air_acceleration, dt);

        if (applyFriction)
        {
            var speed = horizontalVel.Length();
            if (speed > 0.001f)
            {
                var control = MathF.Max(speed, stop_speed);
                var drop = control * (ground_friction) * dt;
                horizontalVel *= MathF.Max(speed - drop, 0f) / speed;
            }
        }

        var limit = MathF.Max(maxSpeed, oldSpeed);
        var newSpeed = horizontalVel.Length();
        if (newSpeed > limit) horizontalVel *= limit / newSpeed;

        Velocity = new Vector3(horizontalVel.X, Velocity.Y, horizontalVel.Z);

        var rightVector = new Vector3(forward.Z, 0f, -forward.X);
        float sideVelocity = Vector3.Dot(Velocity, rightVector);
        float targetTilt = -sideVelocity * camera_lean_amount;

        if (jumping)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            Velocity = Velocity with { Y = MathF.Sqrt(2f * gravity * jump_height) };
        }

        currentTilt = MathUtil.Lerp(currentTilt, targetTilt, frameInfo.DeltaSeconds * camera_lean_responsiveness);
        Camera.Rotation = new Vector3(currentPitch, 0, currentTilt);
    }

    protected internal override bool OnKeyDown(KeyboardInputEvent e)
    {
        if (e.Key == Key.Escape)
        {
            game.Exit();
            return true;
        }

        return base.OnKeyDown(e);
    }

    private static Vector3 accelerate(Vector3 vel, Vector3 wishDir, float wishSpeed, float accel, float dt)
    {
        var add = wishSpeed - Vector3.Dot(vel, wishDir);
        if (add <= 0f) return vel;
        return vel + wishDir * MathF.Min(accel * dt, add);
    }
}
