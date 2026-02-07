using Common.Logger;
using Raylib_cs;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Resources;

namespace Synesthesia.Engine.Threading.Runners;

public class RenderThreadRunner(ThreadType type) : ThreadRunner(type)
{
    private Game game = null!;
    public static Shader SignedDistanceFieldShader;
    public static Shader AlphaShader;

    public Camera3d FallbackCamera { get; private set; } = null!;

    public Camera3d ActiveCamera => game.RootComposite3d.ActiveCamera3d ?? FallbackCamera;

    protected override Logger.LogCategory GetLoggerCategory() => Logger.Render;

    protected override void OnThreadInit(Game game)
    {
        this.game = game;
        Logger.Debug("Loading window host..");
        this.game.WindowsHost.Initialize(this.game);

        // Load resources dependent on gl
        ResourceManager.ResolveAll("ttf");
        ResourceManager.ResolveAll("vsh");
        ResourceManager.ResolveAll("fsh");

        SignedDistanceFieldShader = ResourceManager.Get<Shader>("SynesthesiaResources.Shaders.sdf_font.fsh");
        AlphaShader = ResourceManager.Get<Shader>("SynesthesiaResources.Shaders.alpha.fsh");

        FallbackCamera = new Camera3d();
    }

    protected override void OnLoadComplete(Game game)
    {
    }

    protected override void OnLoop(FrameInfo frameInfo)
    {
        game.WindowsHost.PollEvents();

        PollInputEvents();

        if (Raylib.IsWindowReady() && game.WindowsHost.ShouldWindowClose)
        {
            game.Dispose();
        }

        game.RootComposite2d.Size = game.WindowsHost.WindowSize;
        game.EngineDebugOverlay.Size = game.WindowsHost.WindowSize;

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        Raylib.BeginMode3D(ActiveCamera.RaylibCamera);
        game.RootComposite3d.OnDraw();
        Raylib.EndMode3D();

        game.RootComposite2d.OnDraw();
        game.EngineDebugOverlay.OnDraw();

        Raylib.EndDrawing();
        Raylib.EndBlendMode();

        PollInputEvents();
    }

    private readonly HashSet<KeyboardKey> activeKeys = [];
    private readonly bool[] activeMouseButtons = new bool[6];
    private readonly List<KeyboardKey> releasedKeysBuffer = new(32);

    public void PollInputEvents()
    {
        if(InputSimulator.SimulatingInput) return;

        int key;
        while ((key = Raylib.GetKeyPressed()) != 0)
        {
            var keyboardKey = (KeyboardKey)key;
            InputManager.EnqueueEvent(new KeyInputEvent(keyboardKey, true));
            activeKeys.Add(keyboardKey);
        }

        float wheelDelta;
        if ((wheelDelta = Raylib.GetMouseWheelMove()) != 0f)
        {
            InputManager.EnqueueEvent(new MouseWheelInputEvent(wheelDelta));
        }

        releasedKeysBuffer.Clear();
        foreach (var activeKey in activeKeys.Where(activeKey => Raylib.IsKeyReleased(activeKey)))
        {
            releasedKeysBuffer.Add(activeKey);
        }

        foreach (var activeKey in releasedKeysBuffer)
        {
            activeKeys.Remove(activeKey);
            InputManager.EnqueueEvent(new KeyInputEvent(activeKey, false));
        }

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
        var deltaMousePosition = Raylib.GetMouseDelta();
        if (deltaMousePosition.X != 0 || deltaMousePosition.Y != 0)
        {
            InputManager.LastMousePositionDelta = deltaMousePosition;
            InputManager.LastMousePosition = mousePosition;

            InputManager.EnqueueEvent(new MouseMoveInputEvent(mousePosition, deltaMousePosition));
        }

        int charCode;
        while ((charCode = Raylib.GetCharPressed()) != 0)
        {
            var character = (char)charCode;
            InputManager.EnqueueEvent(new TextInputEvent(character));
        }
    }
}
