using Silk.NET.OpenGL;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Logging;
using SynesthesiaUtil.Extensions;
using static SDL3.SDL;

namespace Synesthesia.Engine.Platform;

public class SDL3WindowHost : IWindowHost
{
    private const int events_per_peep = 64;

    public bool ExitRequested { get; private set; } = false;
    public IntPtr WindowHandle { get; private set; }

    public bool WindowExists { get; private set; } = false;
    public IntPtr GlContext { get; private set; }
    public GL OpenGL { get; private set; } = null!;

    private bool waitingForFirstSwap = true;

    private readonly Event[] events = new Event[events_per_peep];
    public void Initialize()
    {
        SetHint(Hints.AppName, "Synesthesia Engine");

        if (!Init(InitFlags.Video | InitFlags.Gamepad))
        {
            throw new InvalidOperationException($"Failed to initialise SDL: {GetError()}");
        }

        var version = GetVersion();
        var major = VersionNumMajor(version);
        var minor = VersionNumMinor(version);
        var micro = VersionNumMicro(version);
        var revision = GetRevision();
        var videoDriver = GetCurrentVideoDriver();

        Logger.Debug("SDL 3 Initialized");
        Logger.Debug($"- Version:      {major}.{minor}.{micro}");
        Logger.Debug($"- Revision:     {revision}");
        Logger.Debug($"- Video Driver: {videoDriver}");

        var windowCreationFlags = WindowFlags.Resizable |
                                  WindowFlags.HighPixelDensity |
                                  WindowFlags.OpenGL |
                                  WindowFlags.Hidden; // prevent white flash. Unhide after the first swap

        SetHint(Hints.WindowsCloseOnAltF4, "0").LogErrorIfFailed();
        SetHint(Hints.MouseRelativeModeCenter, "0").LogErrorIfFailed();
        SetHint(Hints.IMEImplementedUI, "composition").LogErrorIfFailed();

        IntPtr? windowHandle = CreateWindow("test", IWindowHost.DEFAULT_WIDTH, IWindowHost.DEFAULT_HEIGHT, windowCreationFlags);
        if (windowHandle == null) throw new InvalidOperationException($"Failed to create SDL window. SDL Error: {GetError()}");

        WindowHandle = windowHandle.Value;
        StopTextInput(WindowHandle).LogErrorIfFailed();

        GLSetAttribute(GLAttr.ContextMajorVersion, 3);
        GLSetAttribute(GLAttr.ContextMinorVersion, 3);
        GLSetAttribute(GLAttr.ContextProfileMask, (int)GLProfile.Core);

        IntPtr? glContext = GLCreateContext(WindowHandle);

        if (glContext == null) throw new InvalidOperationException($"Failed to create GL Context. SDL Error: {GetError()}");
        GlContext = glContext.Value;

        GLMakeCurrent(WindowHandle, GlContext).LogErrorIfFailed();

        var gl = GL.GetApi(name =>
        {
            var ptr = GLGetProcAddress(name);
            return ptr;
        });

        OpenGL = gl ?? throw new InvalidOperationException("Silk.NET could not bind to OpenGL");

        WindowExists = true;
        Loop();
    }

    protected void Exit()
    {
        GLDestroyContext(GlContext).LogErrorIfFailed();
        DestroyWindow(GlContext);
        Quit();
    }

    protected void Loop()
    {
        while (WindowExists) ProcessFrame();
        Exit();
    }

    protected void ProcessFrame()
    {
        if (!WindowExists) return;

        if (ExitRequested)
        {
            WindowExists = false;
            return;
        }

        pollSDLEvents();
        OpenGL.ClearColor(0.39f, 0.58f, 0.93f, 1.0f);
        OpenGL.Clear(ClearBufferMask.ColorBufferBit);

        GLSwapWindow(WindowHandle);

        if (waitingForFirstSwap)
        {
            ShowWindow(WindowHandle).LogErrorIfFailed();
            waitingForFirstSwap = false;
        }
        //TODO Poll mouse
    }

    private void pollSDLEvents()
    {
        PumpEvents();

        int eventsRead;

        do
        {
            eventsRead = PeepEvents(events, events_per_peep, EventAction.GetEvent, (uint)EventType.First, (uint)EventType.Last).LogErrorIfFailed();
            for (int i = 0; i < eventsRead; i++)
                HandleEvent(events[i]);
        } while (eventsRead == events_per_peep);
    }

    protected void HandleEvent(Event sdlEvent)
    {
        switch ((EventType)sdlEvent.Type)
        {
            case EventType.Quit:
            {
                ExitRequested = true;
                break;
            }

            case EventType.KeyDown:
            case EventType.KeyUp:
                GlobalInputHandler.HandleKeyboardInput(sdlEvent.Key);
                break;
        }
    }

    public bool CapsLockPressed => GetModState().HasFlagFast(Keymod.Caps);

    public bool AltPressed => GetModState().HasFlagFast(Keymod.Alt);

    public bool HasKeyboard => HasKeyboard();
}
