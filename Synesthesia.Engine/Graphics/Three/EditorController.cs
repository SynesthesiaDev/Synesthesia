// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using System.Numerics;
using Synesthesia.Engine.Components.Two.Debug;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Graphics.Three;

public class EditorController : CompositeDrawable3D
{
    [Singleton]
    private InputHandler inputHandler = null!;

    [Singleton]
    private EngineDebugOverlay debugOverlay = null!;

    public Camera Camera { get; set; } = null!;

    public float Speed { get; set; } = 25f;

    private bool mouseCaptured;

    protected override void OnLoading()
    {
        Children =
        [
            Camera = new Camera(),
        ];

        Renderer.Camera = Camera;
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if (mouseCaptured)
        {
            var delta = InputHandler.ConsumeMouseDelta();
            var sensitivity = 0.1f;

            var currentRot = Rotation;
            currentRot.Y -= delta.X * sensitivity;
            currentRot.X -= delta.Y * sensitivity;

            currentRot.X = Math.Clamp(currentRot.X, -89f, 89f);

            currentRot.Y = (currentRot.Y + 360f) % 360f;

            Rotation = currentRot;
        }
        else
        {
            InputHandler.ConsumeMouseDelta();
        }

        var q = RawRotation;
        var forward = Vector3.Transform(-Vector3.UnitZ, q);
        var right = Vector3.Transform(Vector3.UnitX, q);

        Vector3 moveDir = Vector3.Zero;
        if (Key.W.IsDown()) moveDir += forward;
        if (Key.S.IsDown()) moveDir -= forward;
        if (Key.D.IsDown()) moveDir += right;
        if (Key.A.IsDown()) moveDir -= right;
        if (Key.Space.IsDown()) moveDir += Vector3.UnitY;
        if (Key.Q.IsDown()) moveDir -= Vector3.UnitY;

        if (moveDir != Vector3.Zero)
        {
            var moveSpeed = Key.ShiftLeft.IsDown() ? Speed * 2 : Speed;
            Position += Vector3.Normalize(moveDir) * moveSpeed * frameInfo.DeltaSeconds;
        }

        base.OnUpdate(frameInfo);
    }

    protected internal override bool OnMouseDown(ICursorInputEvent e)
    {
        if (e is MouseButtonInputInputEvent { Button: MouseButton.Right })
        {
            inputHandler.SetMouseCaptured(true);
            mouseCaptured = true;
            return true;
        }

        return base.OnMouseDown(e);
    }

    protected internal override void OnMouseUp(ICursorInputEvent e)
    {
        if (e is MouseButtonInputInputEvent { Button: MouseButton.Right })
        {
            inputHandler.SetMouseCaptured(false);
            mouseCaptured = false;
            return;
        }

        base.OnMouseUp(e);
    }

    protected internal override bool OnMouseWheel(float delta)
    {
        if (!mouseCaptured) return false;

        if (Key.ControlLeft.IsDown())
        {
            if (delta > 0)
                Camera.Fov--;
            else
                Camera.Fov++;

            debugOverlay.PushPopup("Field of View", Camera.Fov.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            if (delta > 0)
                Speed++;
            else
                Speed--;

            Speed = Math.Clamp(Speed, 1f, 100f);
            debugOverlay.PushPopup("Camera Speed", Speed.ToString(CultureInfo.InvariantCulture));
        }

        return true;
    }
}
