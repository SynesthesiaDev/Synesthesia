using System.Numerics;
using Common.Bindable;
using Common.Logger;
using Raylib_cs;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Host;

public class WindowHost : IDisposable
{
    private BindablePool _bindablePool = new();

    public bool ShouldWindowClose => Raylib.WindowShouldClose();

    public Vector2 WindowScaleDPI => Raylib.GetWindowScaleDPI();

    public Bindable<WindowState> WindowState = null!;

    public Bindable<bool> IsFullscreen = null!;

    public Bindable<Vector2> WindowPosition = null!;

    private bool _closing;

    public unsafe void Initialize(Game game)
    {
        
        Raylib.SetTraceLogCallback(&RaylibLoggerProxy.HandleRaylibLog);
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(400, 800, game.WindowTitle.Value);


        WindowState = _bindablePool.Borrow(Host.WindowState.Normal);
        IsFullscreen = _bindablePool.Borrow(EngineEnvironment.StartFullscreen);
        WindowPosition = _bindablePool.Borrow(Raylib.GetWindowPosition());

        WindowState.OnValueChange(e =>
        {
            if (e.NewValue == Host.WindowState.Normal) Raylib.RestoreWindow();
            if (e.NewValue == Host.WindowState.Minimized) Raylib.MinimizeWindow();
            if (e.NewValue == Host.WindowState.Maximized) Raylib.MaximizeWindow();
        }, true);

        WindowPosition.OnValueChange(e => Raylib.SetWindowPosition((int)e.NewValue.X, (int)e.NewValue.Y));

        IsFullscreen.OnValueChange(_ => Raylib.ToggleFullscreen());

        game.WindowTitle.OnValueChange(e => Raylib.SetWindowTitle(e.NewValue));
    }

    public void PollEvents() => Raylib.PollInputEvents();

    public Vector2 WindowSize => new(Raylib.GetRenderWidth(), Raylib.GetRenderHeight());

    public void Close() => Raylib.CloseWindow();

    public void Dispose()
    {
        if (_closing) return;
        _closing = true;
        Logger.Debug("Disposing WindowHost..", Logger.RENDER);
        Close();
        _bindablePool.Dispose();
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