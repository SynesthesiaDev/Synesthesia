using System.Numerics;
using Common.Logger;
using Raylib_cs;
using Synesthesia.Engine.Graphics.Three.Shapes;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Resources;

namespace Synesthesia.Engine.Threading.Runners;

public class RenderThreadRunner : IThreadRunner
{
    private Game _game = null!;
    private Camera3D _camera;

    protected override void OnThreadInit(Game game)
    {
        _game = game;
        Logger.Debug("Loading window host..");
        _game.WindowHost.Initialize(_game);

        ResourceManager.ResolveAll("ttf");

        _camera = new Camera3D
        {
            Position = new Vector3(6f, 6f, 6f),
            Target = Vector3.Zero,
            Up = Vector3.UnitY,
            FovY = 60f,
            Projection = CameraProjection.Perspective
        };

        _game.RootComposite3d.Children =
        [
            new Cube
            {
                Position = new Vector3(0f, 0f, 0f),
                Size = new Vector3(1f, 1f, 1f),
                Color = Color.Red
            },
            new Cube
            {
                Color = Color.Blue,
                Position = new Vector3(2f, 0f, 0f),
                Rotation = new Vector3(45, 0, 0)
            }
        ];
    }

    public override void OnLoadComplete(Game game)
    {
    }

    protected override void OnLoop()
    {
        _game.WindowHost.PollEvents();

        int key;
        while ((key = Raylib.GetKeyPressed()) != 0)
        {
            InputManager.EnqueueKeyEvent((KeyboardKey)key, true);
        }

        if (Raylib.IsWindowReady() && _game.WindowHost.ShouldWindowClose)
        {
            _game.Dispose();
        }

        _game.RootComposite2d.Size = _game.WindowHost.WindowSize;
        _game.EngineDebugOverlay.Size = _game.WindowHost.WindowSize;

        Raylib.UpdateCamera(ref _camera, CameraMode.Orbital);

        Raylib.BeginDrawing();

        Raylib.ClearBackground(Color.Black);
        Raylib.BeginMode3D(_camera);
        Raylib.DrawGrid(20, 1.0f);
        _game.RootComposite3d.OnDraw();
        Raylib.EndMode3D();

        _game.RootComposite2d.OnDraw();
        _game.EngineDebugOverlay.OnDraw();

        // Raylib.DrawTextEx(font, "Hello from Memory!", new System.Numerics.Vector2(100, 100), 32, 2, Color.White);
        Raylib.EndDrawing();
    }
}