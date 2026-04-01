using System.Numerics;
using Synesthesia.Engine.Events;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Util.Bindables;
using Synesthesia.Engine.Util.Future;

namespace Synesthesia.Engine.Platform.Host;

public interface IWindowHost : IDisposable
{
    const int DEFAULT_WIDTH = 1366;
    const int DEFAULT_HEIGHT = 768;

    EventDispatcher<bool> ExitRequested { get; }
    EventDispatcher<Vector2> OnWindowResized { get; }
    EventDispatcher<Nothing> OnDeviceLowMemory { get; }
    EventDispatcher<SystemTheme> OnSystemThemeChanged { get; }
    EventDispatcher<Vector4> OnSafeAreaChanged { get; }
    Bindable<bool> CursorInWindow { get; }
    Bindable<WindowState> WindowState { get; }
    Bindable<bool> WindowActive { get; }

    void Flash(bool flashUntilFocused);

    void CancelFlash();

    OpenGLSurface Surface { get; }

    OpenGlRenderer Renderer { get; }

    bool WindowExists { get; }

    void Initialize();

    void RunWindow();

    bool CapsLockPressed { get; }

    bool AltPressed { get; }

    bool HasKeyboard { get; }

    void Schedule(Action action);

    void Show();

    void Hide();

    IClipboard Clipboard { get; }

}
