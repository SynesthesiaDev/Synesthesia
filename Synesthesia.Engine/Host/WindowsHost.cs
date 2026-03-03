using System.Numerics;
using Common.Bindable;
using Common.Logger;
using Raylib_cs;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Host;

public class WindowsHost : IDisposable
{
    private readonly BindablePool bindablePool = new();

    public bool ShouldWindowClose => Raylib.WindowShouldClose();

    public Vector2 WindowScaleDpi => Raylib.GetWindowScaleDPI();

    public Bindable<WindowState> WindowState = null!;

    public Bindable<bool> IsFullscreen = null!;

    public Bindable<Vector2> WindowPosition = null!;

    public Bindable<bool> WindowFocused = null!;

    private bool closing;

    private Game owningGame = null!;

    public unsafe void Initialize(Game game)
    {
        owningGame = game;

        Raylib.SetTraceLogCallback(&RaylibLoggerProxy.HandleRaylibLog);
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
        Raylib.SetConfigFlags(ConfigFlags.StencilBuffer8Bit);

        Raylib.InitWindow(400, 800, game.WindowTitle.Value);

        Raylib.SetExitKey(KeyboardKey.Null);

        WindowState = bindablePool.Borrow(Host.WindowState.Normal);
        IsFullscreen = bindablePool.Borrow(EngineEnvironment.START_FULLSCREEN);
        WindowPosition = bindablePool.Borrow(Raylib.GetWindowPosition());
        WindowFocused = bindablePool.Borrow<bool>(Raylib.IsWindowFocused());

        WindowState.OnValueChange(e =>
        {
            if (e.NewValue == Host.WindowState.Normal) Raylib.RestoreWindow();
            if (e.NewValue == Host.WindowState.Minimized) Raylib.MinimizeWindow();
            if (e.NewValue == Host.WindowState.Maximized) Raylib.MaximizeWindow();
        }, true);

        IsFullscreen.OnValueChange(_ => Raylib.ToggleFullscreen());

        game.WindowTitle.OnValueChange(e => Raylib.SetWindowTitle(e.NewValue));
    }

    public void PollEvents()
    {
        Raylib.PollInputEvents();

        var windowFocused = Raylib.IsWindowFocused();
        var windowPos = Raylib.GetWindowPosition();

        if (WindowPosition.Value != windowPos) owningGame.UpdateThread.Schedule(() => WindowPosition.Value = windowPos);
        if (WindowFocused.Value != windowFocused) owningGame.UpdateThread.Schedule(() => WindowFocused.Value = windowFocused);
    }

    public Vector2 WindowSize => new(Raylib.GetRenderWidth(), Raylib.GetRenderHeight());

    public void Close() => Raylib.CloseWindow();

    public void Dispose()
    {
        if (closing) return;
        closing = true;
        Logger.Debug("Disposing WindowHost..", Logger.Render);
        Close();
        bindablePool.Dispose();
    }

    public void ToggleFlag(ConfigFlags flag)
    {
        if (Raylib.IsWindowState(flag))
        {
            Raylib.ClearWindowState(flag);
        }
        else
        {
            Raylib.SetWindowState(flag);
        }
    }
}
