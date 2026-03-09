using Common.Logger;
using Raylib_cs;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Fonts;
using Synesthesia.Engine.Graphics.Shaders;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Resources.Stores;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace Synesthesia.Engine.Threading.Runners;

public class RenderThread(ThreadType type, long activeUpdateRate, long inactiveUpdateRate = 60) : ThreadRunner(type, activeUpdateRate, inactiveUpdateRate)
{
    private Game game = null!;

    [Resolved]
    private IResourceStore<Texture> textureStore = null!;

    [Resolved]
    private IResourceStore<Font> fontStore = null!;

    [Resolved]
    private IResourceStore<Shader> shaderStore = null!;

    public Camera3d FallbackCamera { get; private set; } = null!;

    public Camera3d ActiveCamera => game.RootComposite3d.ActiveCamera3d ?? FallbackCamera;

    protected override Logger.LogCategory GetLoggerCategory() => Logger.Render;

    protected override void OnThreadInit(Game game)
    {
        DependencyContainer.Add(this);
        Reflection.ResolveDependencies(this);

        this.game = game;

        Logger.Debug("Loading window host..");
        this.game.WindowsHost.Initialize(this.game);

        (fontStore as DeferredStore<Font>)!.Unlock();
        (textureStore as DeferredStore<Texture>)!.Unlock();
        (shaderStore as DeferredStore<Shader>)!.Unlock();

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

        if (game.RootComposite3d.Children.Any())
        {
            Raylib.BeginMode3D(ActiveCamera.RaylibCamera);
            game.RootComposite3d.OnDraw();
            Raylib.EndMode3D();
        }

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
        if (InputSimulator.SimulatingInput) return;
        var timestamp = DateTimeOffset.Now.Millisecond;

        #region KeyDown

        int key;
        while ((key = Raylib.GetKeyPressed()) != 0)
        {
            var keyboardKey = (KeyboardKey)key;
            var keyEvent = InputManager.KEY_INPUT_EVENT_POOL.Rent();

            keyEvent.Timestamp = timestamp;
            keyEvent.Key = keyboardKey;
            keyEvent.IsDown = true;

            InputManager.EnqueueEvent(keyEvent);
            activeKeys.Add(keyboardKey);
        }

        #endregion

        #region KeyUp

        releasedKeysBuffer.Clear();
        foreach (var activeKey in activeKeys)
        {
            if (Raylib.IsKeyReleased(activeKey))
            {
                releasedKeysBuffer.Add(activeKey);
            }
        }

        foreach (var activeKey in releasedKeysBuffer)
        {
            activeKeys.Remove(activeKey);
            var keyEvent = InputManager.KEY_INPUT_EVENT_POOL.Rent();

            keyEvent.Timestamp = timestamp;
            keyEvent.IsDown = false;
            keyEvent.Key = activeKey;

            InputManager.EnqueueEvent(keyEvent);
        }

        #endregion

        #region MouseScrollWheel

        float wheelDelta;
        if ((wheelDelta = Raylib.GetMouseWheelMove()) != 0f)
        {
            var wheelEvent = InputManager.MOUSE_SCROLL_WHEEL_INPUT_EVENT_POOL.Rent();

            wheelEvent.Timestamp = timestamp;
            wheelEvent.Delta = wheelDelta;

            InputManager.EnqueueEvent(wheelEvent);
        }

        #endregion

        #region MouseButton

        for (var i = 0; i < 6; i++)
        {
            var mouseButton = (MouseButton)i;
            var previousState = activeMouseButtons[i];
            var currentState = Raylib.IsMouseButtonDown(mouseButton);
            if (previousState == currentState) continue;

            var mouseButtonEvent = InputManager.MOUSE_BUTTON_INPUT_EVENT_POOL.Rent();

            mouseButtonEvent.Timestamp = timestamp;
            mouseButtonEvent.IsDown = currentState;
            mouseButtonEvent.Button = mouseButton;

            InputManager.EnqueueEvent(mouseButtonEvent);
            activeMouseButtons[i] = currentState;
        }

        #endregion

        #region MousePosition

        var mousePosition = Raylib.GetMousePosition();
        var deltaMousePosition = Raylib.GetMouseDelta();

        if (deltaMousePosition.X != 0 || deltaMousePosition.Y != 0)
        {
            InputManager.LastMousePositionDelta = deltaMousePosition;
            InputManager.LastMousePosition = mousePosition;

            var mouseMoveEvent = InputManager.MOUSE_MOVE_INPUT_EVENT_POOL.Rent();

            mouseMoveEvent.Timestamp = timestamp;
            mouseMoveEvent.Position = mousePosition;
            mouseMoveEvent.PositionDelta = deltaMousePosition;

            InputManager.EnqueueEvent(mouseMoveEvent);
        }

        #endregion

        #region TextInput

        int charCode;
        while ((charCode = Raylib.GetCharPressed()) != 0)
        {
            var code = charCode;
            game.InputThread.Schedule(() =>
            {
                if (InputManager.FocusedDrawable == null) return;

                var character = (char)code;
                var textEvent = InputManager.TEXT_INPUT_EVENT_POOL.Rent();

                textEvent.Timestamp = timestamp;
                textEvent.Character = character;

                InputManager.EnqueueEvent(textEvent);
            });
        }

        #endregion
    }
}
