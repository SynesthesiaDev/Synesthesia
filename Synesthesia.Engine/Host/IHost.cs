using System.Numerics;
using Synesthesia.Engine.Configuration;
using Veldrid;

namespace Synesthesia.Engine.Host;

public interface IHost
{
    public bool WindowExists { get; set; }

    void Initialize(Game game, RendererType rendererType);

    void Shutdown();

    void SetWindowTitle(string title);

    Vector2 GetWindowSize();

    void SetWindowSize(Vector2 size);

    bool IsWindowFocused();

    bool IsWindowMinimized();

    GraphicsDevice GetGraphicsDevice();

    CommandList GetCommandList();

    void SwapBuffers();

    void PollEvents();

    void SetVsync(bool enabled);

    string GetPlatformName();

    string GetHostName();
}