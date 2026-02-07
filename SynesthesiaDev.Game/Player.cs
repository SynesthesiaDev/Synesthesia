// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Bindable;
using Raylib_cs;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Engine.Input;

namespace SynesthesiaDev.Game;

public class Player : CompositeDrawable3d
{
    private const float jump_height = 5.0f;

    public Camera3d Camera { get; private set; } = null!;

    private readonly BindableProxy bindableProxy = new();

    private Synesthesia.Engine.Game game => DependencyContainer.Get<Synesthesia.Engine.Game>();

    protected override void OnLoading()
    {
        Children =
        [
            Camera = new Camera3d()
        ];

        base.OnLoading();
    }

    const float sensitivity = 0.12f; // degrees per "delta unit" (usually pixels)
    const float maxPitch = 89f;

    bool invertY = false;
    bool cursorCaptured = true;

    private float yawDeg;
    private float pitchDeg;

    protected override void LoadComplete()
    {
        bindableProxy.Subscribe(InputManager.ON_KEY_DOWN, key =>
        {
            if (key == KeyboardKey.Space)
            {
                Position = Position with { Y = Position.Y + jump_height };
            }
        });

        bindableProxy.Subscribe(InputManager.ON_MOUSE_MOVE_DELTA, delta =>
        {
            if(!game.CursorConsumed) return;

            yawDeg -= delta.X * sensitivity;

            var pitchDelta = delta.Y * sensitivity;
            pitchDeg -= invertY ? pitchDelta : -pitchDelta;

            pitchDeg = Math.Clamp(pitchDeg, -maxPitch, maxPitch);

            Rotation = Rotation with { Y = yawDeg };

            Camera.Rotation = Camera.Rotation with { X = pitchDeg, Z = 0f };
        });

        base.LoadComplete();
    }

    protected override void Dispose(bool isDisposing)
    {
        bindableProxy.Dispose();

        base.Dispose(isDisposing);
    }
}
