using Silk.NET.OpenGL;

namespace Synesthesia.Engine.Platform;

public interface IWindowHost
{
    const int DEFAULT_WIDTH = 1366;
    const int DEFAULT_HEIGHT = 768;

    bool ExitRequested { get; }

    IntPtr WindowHandle { get; }

    bool WindowExists { get; }

    IntPtr GlContext { get; }

    GL OpenGL { get; }

    void Initialize();

    bool CapsLockPressed { get; }

    bool AltPressed { get; }

    bool HasKeyboard { get; }
}
