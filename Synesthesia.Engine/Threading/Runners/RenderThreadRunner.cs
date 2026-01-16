using System.Numerics;
using Common.Logger;
using Raylib_cs;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Resources;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Threading.Runners;

public class RenderThreadRunner : IThreadRunner
{
    private Game _game = null!;
    private Camera3D _camera;
    public static Shader SignedDistanceFieldShader;
    public static Shader AlphaShader;

    protected override void OnThreadInit(Game game)
    {
        _game = game;
        Logger.Debug("Loading window host..");
        _game.WindowHost.Initialize(_game);

        // Load resources dependent on gl
        ResourceManager.ResolveAll("ttf");
        ResourceManager.ResolveAll("vsh");
        ResourceManager.ResolveAll("fsh");

        SignedDistanceFieldShader = ResourceManager.Get<Shader>("SynesthesiaResources.Shaders.sdf_font.fsh");
        AlphaShader = ResourceManager.Get<Shader>("SynesthesiaResources.Shaders.alpha.fsh");

        _camera = new Camera3D
        {
            Position = new Vector3(6f, 6f, 6f),
            Target = Vector3.Zero,
            Up = Vector3.UnitY,
            FovY = 60f,
            Projection = CameraProjection.Perspective,
        };

        // _game.RootComposite3d.Children =
        // [
        //     new Cube
        //     {
        //         Position = new Vector3(0f, 0f, 0f),
        //         Size = new Vector3(1f, 1f, 1f),
        //         Color = Color.Red
        //     },
        //     new Cube
        //     {
        //         Color = Color.Blue,
        //         Position = new Vector3(2f, 0f, 0f),
        //         Rotation = new Vector3(45, 0, 0)
        //     }
        // ];
    }

    public override void OnLoadComplete(Game game)
    {
    }

    protected override void OnLoop()
    {
        _game.WindowHost.PollEvents();

        pollKeyboardEvents();

        if (Raylib.IsWindowReady() && _game.WindowHost.ShouldWindowClose)
        {
            _game.Dispose();
        }

        _game.RootComposite2d.Size = _game.WindowHost.WindowSize;
        _game.EngineDebugOverlay.Size = _game.WindowHost.WindowSize;

        Raylib.UpdateCamera(ref _camera, CameraMode.Custom);

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        Raylib.BeginMode3D(_camera);
        // Raylib.DrawGrid(20, 1.0f);
        _game.RootComposite3d.OnDraw();
        Raylib.EndMode3D();

        _game.RootComposite2d.OnDraw();
        _game.EngineDebugOverlay.OnDraw();

        Raylib.EndDrawing();
        Raylib.EndBlendMode();

        pollKeyboardEvents();
    }

    private readonly HashSet<KeyboardKey> _activeKeys = [];
    private readonly bool[] _activeMouseButtons = new bool[6];
    private Vector2 _lastMousePos = Vector2.Zero;

    private void pollKeyboardEvents()
    {
        int key;
        while ((key = Raylib.GetKeyPressed()) != 0)
        {
            var keyboardKey = (KeyboardKey)key;
            InputManager.EnqueueEvent(new KeyInputEvent(keyboardKey, true));
            _activeKeys.Add(keyboardKey);
        }

        _activeKeys.ToList().Filter(k => Raylib.IsKeyReleased(k)).ForEach(keyboardKey =>
        {
            _activeKeys.Remove(keyboardKey);
            InputManager.EnqueueEvent(new KeyInputEvent(keyboardKey, false));
        });

        for (var i = 0; i < 6; i++)
        {
            var mouseButton = (MouseButton)i;
            var previousState = _activeMouseButtons[i];
            var currentState = Raylib.IsMouseButtonDown(mouseButton);
            if (previousState == currentState) continue;
            
            InputManager.EnqueueEvent(new MouseButtonInputEvent(mouseButton, currentState));
            _activeMouseButtons[i] = currentState;
        }
        
        var mousePosition = Raylib.GetMousePosition();
        if (mousePosition != _lastMousePos)
        {
            _lastMousePos = mousePosition;
            InputManager.EnqueueEvent(new MouseMoveInputEvent(mousePosition));
        }
    }
}