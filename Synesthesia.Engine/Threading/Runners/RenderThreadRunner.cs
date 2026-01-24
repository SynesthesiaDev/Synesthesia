using System.Numerics;
using Common.Logger;
using Raylib_cs;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Resources;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Threading.Runners;

public class RenderThreadRunner : ThreadRunner
{
    private Game game = null!;
    private Camera3D camera;
    public static Shader SignedDistanceFieldShader;
    public static Shader AlphaShader;

    protected override Logger.LogCategory GetLoggerCategory() => Logger.Render;

    protected override void OnThreadInit(Game game)
    {
        this.game = game;
        Logger.Debug("Loading window host..");
        this.game.WindowHost.Initialize(this.game);

        // Load resources dependent on gl
        ResourceManager.ResolveAll("ttf");
        ResourceManager.ResolveAll("vsh");
        ResourceManager.ResolveAll("fsh");

        SignedDistanceFieldShader = ResourceManager.Get<Shader>("SynesthesiaResources.Shaders.sdf_font.fsh");
        AlphaShader = ResourceManager.Get<Shader>("SynesthesiaResources.Shaders.alpha.fsh");

        camera = new Camera3D
        {
            Position = new Vector3(6f, 6f, 6f),
            Target = Vector3.Zero,
            Up = Vector3.UnitY,
            FovY = 60f,
            Projection = CameraProjection.Perspective,
        };
    }

    protected override void OnLoadComplete(Game game)
    {
    }

    protected override void OnLoop()
    {
        game.WindowHost.PollEvents();

        pollKeyboardEvents();

        if (Raylib.IsWindowReady() && game.WindowHost.ShouldWindowClose)
        {
            game.Dispose();
        }

        game.RootComposite2d.Size = game.WindowHost.WindowSize;
        game.EngineDebugOverlay.Size = game.WindowHost.WindowSize;

        Raylib.UpdateCamera(ref camera, CameraMode.Custom);

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        Raylib.BeginMode3D(camera);
        // Raylib.DrawGrid(20, 1.0f);
        game.RootComposite3d.OnDraw();
        Raylib.EndMode3D();

        game.RootComposite2d.OnDraw();
        game.EngineDebugOverlay.OnDraw();

        Raylib.EndDrawing();
        Raylib.EndBlendMode();

        pollKeyboardEvents();
    }

    private readonly HashSet<KeyboardKey> activeKeys = [];
    private readonly bool[] activeMouseButtons = new bool[6];

    private void pollKeyboardEvents()
    {
        int key;
        while ((key = Raylib.GetKeyPressed()) != 0)
        {
            var keyboardKey = (KeyboardKey)key;
            InputManager.EnqueueEvent(new KeyInputEvent(keyboardKey, true));
            activeKeys.Add(keyboardKey);
        }

        activeKeys.ToList().Filter(k => Raylib.IsKeyReleased(k)).ForEach(keyboardKey =>
        {
            activeKeys.Remove(keyboardKey);
            InputManager.EnqueueEvent(new KeyInputEvent(keyboardKey, false));
        });

        for (var i = 0; i < 6; i++)
        {
            var mouseButton = (MouseButton)i;
            var previousState = activeMouseButtons[i];
            var currentState = Raylib.IsMouseButtonDown(mouseButton);
            if (previousState == currentState) continue;

            InputManager.EnqueueEvent(new MouseButtonInputEvent(mouseButton, currentState));
            activeMouseButtons[i] = currentState;
        }

        var mousePosition = Raylib.GetMousePosition();
        if (mousePosition != InputManager.LastMousePosition)
        {
            InputManager.LastMousePosition = mousePosition;
            InputManager.EnqueueEvent(new MouseMoveInputEvent(mousePosition));
        }

        int charCode;
        while ((charCode = Raylib.GetCharPressed()) != 0)
        {
            var character = (char)charCode;
            InputManager.EnqueueEvent(new TextInputEvent(character));
        }
    }
}
