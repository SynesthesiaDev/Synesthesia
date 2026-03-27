// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using OpenTabletDriver.Plugin.Tablet;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Events;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Util.Bindables;
using Synesthesia.Engine.Util.Future;
using SynesthesiaUtil;
using SynesthesiaUtil.Extensions;
using static SDL3.SDL;

namespace Synesthesia.Engine.Platform.Host;

public class SDL3WindowHost : IWindowHost
{
    private const int events_per_peep = 64;

    private const WindowFlags window_creation_flags = WindowFlags.Resizable |
                                                      WindowFlags.HighPixelDensity |
                                                      WindowFlags.OpenGL |
                                                      WindowFlags.Hidden; // prevent white flash. Unhide after the first swap

    private const MouseButtonFlags valid_buttons_mask =
        MouseButtonFlags.Left | MouseButtonFlags.Right | MouseButtonFlags.Middle |
        MouseButtonFlags.X1 | MouseButtonFlags.X2;

    private readonly ConcurrentQueue<Action> commandQueue = new();

    private readonly BindableEventSource windowStateEventSource = new BindableEventSource();

    #region Events

    public EventDispatcher<bool> ExitRequested { get; } = new();

    public EventDispatcher<Vector2> OnWindowResized { get; } = new EventDispatcher<Vector2>();

    public EventDispatcher<Nothing> OnDeviceLowMemory { get; } = new EventDispatcher<Nothing>();

    public EventDispatcher<SystemTheme> OnSystemThemeChanged { get; } = new EventDispatcher<SystemTheme>();

    public EventDispatcher<Vector4> OnSafeAreaChanged { get; } = new EventDispatcher<Vector4>();

    public Bindable<bool> CursorInWindow { get; } = new Bindable<bool>(false);

    public Bindable<WindowState> WindowState { get; } = new Bindable<WindowState>(Platform.WindowState.Normal);

    public Bindable<bool> WindowActive { get; } = new Bindable<bool>(false);

    #endregion

    public Vector2 Size { get; private set; } = Vector2.Zero;

    public bool Resizable { get; set; } = true;


    /// <summary>
    /// OpenGL Surface containing <see cref="OpenGLSurface.WindowHandle"/>, <see cref="OpenGLSurface.ContextHandle"/> and responsible
    /// for swapping buffers and managing ownership of OpenGL Context
    /// </summary>
    public OpenGLSurface Surface { get; private set; } = null!;

    /// <summary>
    /// Manages everything related to rendering/drawing
    /// </summary>
    public OpenGlRenderer Renderer { get; private set; } = null!;

    public bool WindowExists { get; private set; }

    private readonly Event[] events = new Event[events_per_peep];

    private volatile uint pressedMouseButtons;

    private PointF previousMousePolledPoint = PointF.Empty;

    public bool IsWayland => string.Equals(GetCurrentVideoDriver(), "wayland", StringComparison.Ordinal);

    /// <summary>
    /// Title of the window
    /// </summary>
    public string Title
    {
        get;
        set
        {
            field = value;
            Schedule(() => SetWindowTitle(Surface.WindowHandle, value).LogErrorIfFailed());
        }
    } = Assembly.GetAssembly(typeof(Game))?.FullName ?? "Unknown Assembly";

    /// <summary>
    /// Window Size (in pixels)
    /// </summary>
    public Vector2 WindowSize
    {
        get
        {
            GetWindowSizeInPixels(Surface.WindowHandle, out var x, out var y).ThrowIfFailed();
            return new Vector2(x, y);
        }
    }

    public bool CapsLockPressed => GetModState().HasFlagFast(Keymod.Caps);

    public bool AltPressed => GetModState().HasFlagFast(Keymod.Alt);

    public bool HasKeyboard => HasKeyboard();

    [Singleton]
    private InputHandler inputHandler = null!;

    /// <summary>
    /// Schedule an action to be run on the next SDL frame
    /// </summary>
    /// <param name="action">Action to be run</param>
    public void Schedule(Action action)
    {
        commandQueue.Enqueue(action);
    }

    /// <summary>
    /// Makes the window flash in the taskbar (if supported on os)
    /// </summary>
    /// <param name="flashUntilFocused">Flash until the window is focused</param>
    public void Flash(bool flashUntilFocused) =>
        Schedule(() =>
        {
            if (!RuntimeInfo.IsDesktop) return;
            FlashWindow(Surface.WindowHandle, flashUntilFocused ? FlashOperation.UntilFocused : FlashOperation.Briefly).LogErrorIfFailed();
        });

    /// <summary>
    /// Cancels flashing animation in taskbar if <see cref="Flash"/> was previously called with parameter <c>flashUntilFocused</c>
    /// </summary>
    public void CancelFlash() =>
        Schedule(() =>
        {
            if (!RuntimeInfo.IsDesktop) return;
            FlashWindow(Surface.WindowHandle, FlashOperation.Cancel);
        });

    #region Initialization

    /// <summary>
    /// Initializes SDL3 and creates <see cref="OpenGLSurface"/>, <see cref="OpenGlRenderer"/> and <see cref="TabletDriver"/>
    /// </summary>
    /// <exception cref="InvalidOperationException">Failed to create SDL window</exception>
    /// <exception cref="InvalidOperationException">Failed to create GL Context</exception>
    public void Initialize()
    {
        try
        {
            Reflection.ResolveDependencies(this);

            SetHint(Hints.AppName, Title).LogErrorIfFailed();

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

            Logger.Debug("SDL 3 Initialized", Logger.Platform);
            Logger.Debug($"- Version:         {major}.{minor}.{micro}", Logger.Platform);
            Logger.Debug($"- Revision:        {revision}", Logger.Platform);
            Logger.Debug($"- Video Driver:    {videoDriver}", Logger.Platform);

            SetLogOutputFunction(Logger.SDLLog, IntPtr.Zero);

            SetHint(Hints.WindowsCloseOnAltF4, "0").LogErrorIfFailed();
            SetHint(Hints.MouseRelativeModeCenter, "0").LogErrorIfFailed();
            SetHint(Hints.IMEImplementedUI, "composition").LogErrorIfFailed();

            IntPtr? windowHandle = CreateWindow(Title, IWindowHost.DEFAULT_WIDTH, IWindowHost.DEFAULT_HEIGHT, window_creation_flags);
            if (windowHandle == null) throw new InvalidOperationException($"Failed to create SDL window. SDL Error: {GetError()}");

            StopTextInput(windowHandle.Value).LogErrorIfFailed();

            GLSetAttribute(GLAttr.ContextMajorVersion, 3).LogErrorIfFailed();
            GLSetAttribute(GLAttr.ContextMinorVersion, 3).LogErrorIfFailed();
            GLSetAttribute(GLAttr.ContextProfileMask, (int)GLProfile.Core).LogErrorIfFailed();
            GLSetAttribute(GLAttr.StencilSize, 8).LogErrorIfFailed();

            IntPtr? glContext = GLCreateContext(windowHandle.Value);

            if (glContext == null) throw new InvalidOperationException($"Failed to create GL Context. SDL Error: {GetError()}");

            GLSetSwapInterval(0).LogErrorIfFailed();

            Surface = new OpenGLSurface
            {
                WindowHandle = windowHandle.Value,
                ContextHandle = glContext.Value
            };

            Surface.ClaimOwnership();

            Renderer = new OpenGlRenderer
            {
                Surface = Surface,
            };

            WindowState.OnValueChange(e => updateWindowState(e.NewValue), ignoresSource: windowStateEventSource);

            Renderer.Initialize();

            var driver = TabletDriver.Create();
            driver.DeviceReported += handleTabletDeviceReport;
        }
        catch (Exception exception)
        {
            Logger.Exception(exception, Logger.Platform);
            Environment.Exit(exception.HResult);
        }
    }

    /// <summary>
    /// Marks the window as ready and starts pumping window events
    /// Do mind that the window starts as hidden until the first swap to prevent flashing the user with a white empty window
    /// </summary>
    public void RunWindow()
    {
        WindowExists = true;
        Loop();
    }

    #endregion

    #region Event Loop

    protected void Loop()
    {
        try
        {
            while (WindowExists) ProcessFrame();
        }
        catch (Exception exception)
        {
            Logger.Exception(exception, Logger.Platform);
        }

        Dispose();
    }

    protected void ProcessFrame()
    {
        if (!WindowExists) return;
        while (!commandQueue.IsEmpty)
        {
            if (commandQueue.TryDequeue(out var action))
            {
                action.Invoke();
            }
        }

        pollEvents();
        pollMouse();
    }

    #endregion

    #region Polling

    private void pollEvents()
    {
        PumpEvents();

        int eventsRead;

        do
        {
            eventsRead = PeepEvents(events, events_per_peep, EventAction.GetEvent, (uint)EventType.First, (uint)EventType.Last).LogErrorIfFailed();
            foreach (var sdlEvent in events) HandleEvent(sdlEvent);
        } while (eventsRead == events_per_peep);
    }

    [SuppressMessage("Usage", "MA0099:Use Explicit enum value instead of 0")]
    private void pollMouse()
    {
        var pressed = (MouseButtonFlags)pressedMouseButtons;
        var globalButtons = GetGlobalMouseState(out var x, out var y);

        if (previousMousePolledPoint.X != x || previousMousePolledPoint.Y != y)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            previousMousePolledPoint = new PointF(x, y);
            GetWindowPosition(Surface.WindowHandle, out var posX, out var posY).LogErrorIfFailed();

            var mouseEvent = MouseMoveInputEvent.Rent();
            mouseEvent.Position = new Vector2(x - posX, y - posY);
            mouseEvent.Timestamp = timestamp;

            inputHandler.Enqueue(mouseEvent);
        }

        var buttonsToRelease = (pressed & ~globalButtons) & valid_buttons_mask;

        if (buttonsToRelease != 0)
        {
            Interlocked.And(ref pressedMouseButtons, (uint)~buttonsToRelease);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (buttonsToRelease.HasFlagFast(MouseButtonFlags.Left)) handleMouseButton(MouseButton.Left, false, timestamp);
            if (buttonsToRelease.HasFlagFast(MouseButtonFlags.Middle)) handleMouseButton(MouseButton.Middle, false, timestamp);
            if (buttonsToRelease.HasFlagFast(MouseButtonFlags.Right)) handleMouseButton(MouseButton.Right, false, timestamp);
            if (buttonsToRelease.HasFlagFast(MouseButtonFlags.X1)) handleMouseButton(MouseButton.Button1, false, timestamp);
            if (buttonsToRelease.HasFlagFast(MouseButtonFlags.X2)) handleMouseButton(MouseButton.Button2, false, timestamp);
        }
    }

    #endregion

    #region Window State

    public void Raise() =>
        Schedule(() =>
        {
            var flags = GetWindowFlags(Surface.WindowHandle);

            if (flags.HasFlagFast(WindowFlags.Minimized))
                RestoreWindow(Surface.WindowHandle).LogErrorIfFailed();

            RaiseWindow(Surface.WindowHandle).LogErrorIfFailed();
        });

    public void Hide() => Schedule(() => HideWindow(Surface.WindowHandle).LogErrorIfFailed());

    public void Show() => Schedule(() => ShowWindow(Surface.WindowHandle).LogErrorIfFailed());

    public void EnableScreenSuspension() => Schedule(() => EnableScreenSaver().LogErrorIfFailed());

    public void DisableScreenSuspension() => Schedule(() => DisableScreenSaver().LogErrorIfFailed());

    private void fetchCurrentWindowState()
    {
        var handle = Surface.WindowHandle;
        var flags = GetWindowFlags(handle);

        if (flags.HasFlagFast(WindowFlags.Fullscreen))
        {
            WindowState.Set(Platform.WindowState.Fullscreen, windowStateEventSource);
        }
        else if (flags.HasFlagFast(WindowFlags.Maximized))
        {
            WindowState.Set(Platform.WindowState.Maximised, windowStateEventSource);
        }
        else if (flags.HasFlagFast(WindowFlags.Minimized))
        {
            WindowState.Set(Platform.WindowState.Minimised, windowStateEventSource);
        }
        else
        {
            WindowState.Set(Platform.WindowState.Normal, windowStateEventSource);
        }

        GetWindowSizeInPixels(handle, out int w, out int h);
        Renderer.Resize(w, h);
    }

    private void updateWindowState(WindowState windowState)
    {
        var handle = Surface.WindowHandle;
        switch (windowState)
        {
            case Platform.WindowState.Normal:
                RestoreWindow(handle).LogErrorIfFailed();
                SetWindowSize(handle, (int)Size.X, (int)Size.Y).LogErrorIfFailed();
                SetWindowResizable(handle, Resizable).LogErrorIfFailed();
                break;

            case Platform.WindowState.Fullscreen:
                throw new NotSupportedException();

            case Platform.WindowState.FullscreenBorderless:
                throw new NotSupportedException();

            case Platform.WindowState.Maximised:
                RestoreWindow(handle).LogErrorIfFailed();
                MaximizeWindow(handle).LogErrorIfFailed();
                break;
            case Platform.WindowState.Minimised:
                MinimizeWindow(handle).LogErrorIfFailed();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(windowState), windowState, null);
        }
    }

    #endregion

    #region Event Handling

    protected void HandleEvent(Event sdlEvent)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        switch ((EventType)sdlEvent.Type)
        {
            case EventType.Quit:
            {
                ExitRequested.Dispatch(true);
                WindowExists = false;
                break;
            }

            case EventType.KeyDown:
            case EventType.KeyUp:
                var keyEvent = KeyboardInputEvent.Rent();
                keyEvent.IsDown = sdlEvent.Key.Down;
                keyEvent.Key = sdlEvent.Key.ToKey();
                keyEvent.Timestamp = timestamp;
                inputHandler.Enqueue(keyEvent);
                break;

            // case EventType.TextEditing:
            // GlobalInputHandler.HandleTextEditing(sdlEvent.Edit);
            // break;

            // case EventType.TextInput:
            // GlobalInputHandler.HandleTextInput(sdlEvent.Text);
            // break;

            // case EventType.KeymapChanged:
            // GlobalInputHandler.HandleKeymapChange();
            // break;

            case EventType.FingerDown:
            case EventType.FingerUp:
            case EventType.FingerMotion:
            case EventType.FingerCanceled:
                var touchEvent = TouchInputEvent.Rent();
                touchEvent.Timestamp = timestamp;
                touchEvent.IsDown = sdlEvent.TFinger.Type == EventType.FingerDown;
                touchEvent.Pressure = sdlEvent.TFinger.Pressure;
                touchEvent.FingerId = sdlEvent.TFinger.FingerID;
                break;

            case EventType.DropBegin:
            case EventType.DropComplete:
            case EventType.DropFile:
            case EventType.DropPosition:
            case EventType.DropText:
                // InputHandler.HandleDrop(sdlEvent.Drop);
                break;

            case EventType.MouseButtonDown:
            case EventType.MouseButtonUp:
                handleInternalMouseButtonEvent(sdlEvent.Button);
                break;

            case EventType.MouseMotion:
                handleInternalMouseMotionEvent(sdlEvent.Motion);
                break;
            case EventType.WindowResized:
                OnWindowResized.Dispatch(WindowSize);
                fetchCurrentWindowState();
                break;
            case EventType.LowMemory:
                OnDeviceLowMemory.Dispatch(Nothing.INSTANCE);
                break;
            case EventType.WindowSafeAreaChanged:
                GetWindowSafeArea(Surface.WindowHandle, out var rect);
                OnSafeAreaChanged.Dispatch(rect.ToVector());
                break;
            case EventType.WindowMinimized:
            case EventType.WindowMaximized:
            case EventType.WindowRestored:
                fetchCurrentWindowState();
                break;

            case EventType.SystemThemeChanged:
                OnSystemThemeChanged.Dispatch(GetSystemTheme().ToSystemTheme());
                break;

            case EventType.WindowFocusLost:
                WindowActive.Value = false;
                break;
            case EventType.WindowFocusGained:
                WindowActive.Value = true;
                break;
        }
    }

    private void handleInternalMouseButtonEvent(MouseButtonEvent mouseButtonEvent)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var mouseButton = mouseButtonEvent.ToMouseButton();
        var mask = ButtonMask(mouseButtonEvent.Button);

        var mousePressEvent = MouseButtonInputEvent.Rent();
        switch (mouseButtonEvent.Type)
        {
            case EventType.MouseButtonDown:
                mousePressEvent.Timestamp = timestamp;
                mousePressEvent.Button = mouseButton;
                mousePressEvent.IsDown = true;
                inputHandler.Enqueue(mousePressEvent);

                Interlocked.And(ref pressedMouseButtons, mask);
                break;
            case EventType.MouseButtonUp:
                mousePressEvent.Timestamp = timestamp;
                mousePressEvent.Button = mouseButton;
                mousePressEvent.IsDown = false;
                inputHandler.Enqueue(mousePressEvent);

                Interlocked.And(ref pressedMouseButtons, ~mask);
                break;
            default:
                mousePressEvent.ReturnToPool();
                break;
        }
    }

    private void handleInternalMouseMotionEvent(MouseMotionEvent mouseMotionEvent)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (!GetWindowRelativeMouseMode(Surface.WindowHandle))
        {
            var mouseEvent = MouseMoveInputEvent.Rent();
            mouseEvent.Timestamp = timestamp;
            mouseEvent.Position = new Vector2(mouseMotionEvent.X, mouseMotionEvent.Y);

            inputHandler.Enqueue(mouseEvent);
        }
        else
        {
            var mouseEvent = MouseMoveInputEvent.Rent();
            mouseEvent.Timestamp = timestamp;
            mouseEvent.PositionDelta = new Vector2(mouseMotionEvent.X, mouseMotionEvent.Y);

            inputHandler.Enqueue(mouseEvent);
        }
    }

    private void handleTabletDeviceReport(object? _, IDeviceReport deviceReport)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (deviceReport is IAbsolutePositionReport positionReport)
        {
            var tabletEvent = TabletInputEvent.Rent();
            tabletEvent.Position = positionReport.Position;
            tabletEvent.Timestamp = timestamp;

            inputHandler.Enqueue(tabletEvent);
        }
    }

    private void handleMouseButton(MouseButton mouseButton, bool down, long timestamp)
    {
        var mouseButtonInputEvent = MouseButtonInputEvent.Rent();
        mouseButtonInputEvent.Button = mouseButton;
        mouseButtonInputEvent.IsDown = down;
        mouseButtonInputEvent.Timestamp = timestamp;

        inputHandler.Enqueue(mouseButtonInputEvent);
    }

    #endregion

    public void Dispose()
    {
        Logger.Debug("Exiting..", Logger.Platform);
        ExitRequested.Dispose();
        Surface.Dispose();
        DestroyWindow(Surface.WindowHandle);
        Quit();
    }
}
