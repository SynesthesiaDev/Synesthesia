using System.Numerics;
using Common.Logger;
using Common.Util;
using Synesthesia.Engine.Configuration;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Synesthesia.Engine.Host;

public class SDL2WindowHost : IHost
{
    private Game _game = null!;
    private Sdl2Window _window = null!;
    private GraphicsDevice _graphicsDevice = null!;
    private CommandList _commandList = null!;

    public bool WindowExists { get; private set; }

    public void Initialize(Game game, RendererType rendererType)
    {
        _game = game;
        var windowCI = new WindowCreateInfo
        {
            X = 100,
            Y = 100,
            WindowWidth = 960,
            WindowHeight = 540,
            WindowTitle = _game.WindowTitle,
        };

        var options = new GraphicsDeviceOptions
        {
            HasMainSwapchain = true,
            SwapchainDepthFormat = PixelFormat.R16_UNorm,
            SyncToVerticalBlank = true,
            ResourceBindingModel = ResourceBindingModel.Improved
        };

        _window = VeldridStartup.CreateWindow(ref windowCI);
        Logger.Debug("SDL2 Initialized", Logger.RENDER);

        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, options, RendererTypeInfo.GetGraphicsBackend(rendererType));

        Logger.Debug($"SDL2 Version: {_graphicsDevice.ApiVersion.ToString()}", Logger.RENDER);
        Logger.Debug($"SDL2 Driver: {PlatformUtils.GetPlatformName()}", Logger.RENDER);

        _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();
    }

    public void Shutdown()
    {
        _graphicsDevice.Dispose();
        _window.Close();
    }

    public void SetWindowSize(Vector2 size)
    {
        _window.Height = (int)size.Y;
        _window.Width = (int)size.X;
    }

    public void SetWindowTitle(string title) => _window.Title = title;

    public Vector2 GetWindowSize() => new(_window.Width, _window.Height);

    public bool IsWindowFocused() => _window.Focused;

    public bool IsWindowMinimized() => !_window.Visible;

    public GraphicsDevice GetGraphicsDevice() => _graphicsDevice;

    public CommandList GetCommandList() => _commandList;

    public void SwapBuffers() => _graphicsDevice.SwapBuffers();

    public void SetVsync(bool enabled) => _graphicsDevice.SyncToVerticalBlank = enabled;

    public string GetHostName() => "SDL2";

    public string GetPlatformName() => PlatformUtils.GetPlatformName();

    public void PollEvents() => _window.PumpEvents();
}