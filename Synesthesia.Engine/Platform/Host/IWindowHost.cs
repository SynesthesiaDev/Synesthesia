using Synesthesia.Engine.Events;
using Synesthesia.Engine.Platform.Render;

namespace Synesthesia.Engine.Platform.Host;

public interface IWindowHost
{
    const int DEFAULT_WIDTH = 1366;
    const int DEFAULT_HEIGHT = 768;

    EventDispatcher<bool> ExitRequested { get; }

    void Flash(bool flashUntilFocused);

    void CancelFlash();

    OpenGLSurface Surface { get; }

    bool WindowExists { get; }

    void Initialize();

    bool CapsLockPressed { get; }

    bool AltPressed { get; }

    bool HasKeyboard { get; }
}
