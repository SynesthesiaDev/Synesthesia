// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;
using Synesthesia.Engine.Events;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Render;
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

    private readonly ConcurrentQueue<Action> commandQueue = new();

    public EventDispatcher<bool> ExitRequested { get; } = new();

    public OpenGLSurface Surface { get; private set; } = null!;

    public OpenGlRenderer Renderer { get; private set; } = null!;

    public bool WindowExists { get; private set; }

    private bool waitingForFirstSwap = true;

    private readonly Event[] events = new Event[events_per_peep];

    private volatile uint pressedMouseButtons;

    private PointF previousMousePolledPoint = PointF.Empty;

    public void Schedule(Action action)
    {
        commandQueue.Enqueue(action);
    }

    public void Flash(bool flashUntilFocused) =>
        Schedule(() =>
        {
            if (!RuntimeInfo.IsDesktop) return;
            FlashWindow(Surface.WindowHandle, flashUntilFocused ? FlashOperation.UntilFocused : FlashOperation.Briefly).LogErrorIfFailed();
        });

    public void CancelFlash() =>
        Schedule(() =>
        {
            if (!RuntimeInfo.IsDesktop) return;
            FlashWindow(Surface.WindowHandle, FlashOperation.Cancel);
        });

    public void Initialize()
    {
        try
        {
            SetHint(Hints.AppName, "Synesthesia Engine").LogErrorIfFailed();

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

            SetLogOutputFunction(sdlLog, IntPtr.Zero);

            SetHint(Hints.WindowsCloseOnAltF4, "0").LogErrorIfFailed();
            SetHint(Hints.MouseRelativeModeCenter, "0").LogErrorIfFailed();
            SetHint(Hints.IMEImplementedUI, "composition").LogErrorIfFailed();

            IntPtr? windowHandle = CreateWindow("test", IWindowHost.DEFAULT_WIDTH, IWindowHost.DEFAULT_HEIGHT, window_creation_flags);
            if (windowHandle == null) throw new InvalidOperationException($"Failed to create SDL window. SDL Error: {GetError()}");

            StopTextInput(windowHandle.Value).LogErrorIfFailed();

            GLSetAttribute(GLAttr.ContextMajorVersion, 3).LogErrorIfFailed();
            GLSetAttribute(GLAttr.ContextMinorVersion, 3).LogErrorIfFailed();
            GLSetAttribute(GLAttr.ContextProfileMask, (int)GLProfile.Core).LogErrorIfFailed();
            GLSetAttribute(GLAttr.StencilSize, 8).LogErrorIfFailed();

            IntPtr? glContext = GLCreateContext(windowHandle.Value);

            if (glContext == null) throw new InvalidOperationException($"Failed to create GL Context. SDL Error: {GetError()}");

            Surface = new OpenGLSurface
            {
                WindowHandle = windowHandle.Value,
                ContextHandle = glContext.Value
            };

            Surface.MakeCurrent();

            Renderer = new OpenGlRenderer
            {
                Surface = Surface
            };

            Renderer.Initialize();

            var driver = TabletDriver.Create();
            driver.DeviceReported += handleTabletDeviceReport;

            WindowExists = true;
            Loop();
        }
        catch (Exception exception)
        {
            Logger.Exception(exception, Logger.Platform);
            Environment.Exit(exception.HResult);
        }
    }

    private static void sdlLog(IntPtr userData, LogCategory category, LogPriority priority, string message)
    {
        Logger.Verbose(message, Logger.Platform);
    }

    protected void Exit()
    {
        Logger.Debug("Exiting..", Logger.Platform);
        ExitRequested.Dispose();
        Surface.Dispose();
        DestroyWindow(Surface.WindowHandle);
        Quit();
    }

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

        Exit();
    }

    protected void ProcessFrame()
    {
        if (!WindowExists) return;

        if (commandQueue.TryDequeue(out var action))
        {
            action.Invoke();
        }

        pollEvents();
        if (Renderer.CanDraw)
        {
            Renderer.BeginDrawing();
            Renderer.OpenGL.ClearColor(0.39f, 0.58f, 0.93f, 1.0f);
            Renderer.EndDrawing();
        }

        if (waitingForFirstSwap)
        {
            ShowWindow(Surface.WindowHandle).LogErrorIfFailed();
            waitingForFirstSwap = false;
        }

        pollMouse();
    }

    private void pollEvents()
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

    private const MouseButtonFlags valid_buttons_mask =
        MouseButtonFlags.Left | MouseButtonFlags.Right | MouseButtonFlags.Middle |
        MouseButtonFlags.X1 | MouseButtonFlags.X2;

    private void handleTabletDeviceReport(object? _, IDeviceReport deviceReport)
    {
        if (deviceReport is IAbsolutePositionReport positionReport)
        {
            GlobalInputHandler.HandlePenMotion(positionReport.Position);
        }
    }

    private void pollMouse()
    {
        var pressed = (MouseButtonFlags)pressedMouseButtons;
        var globalButtons = GetGlobalMouseState(out var x, out var y);

        if (previousMousePolledPoint.X != x || previousMousePolledPoint.Y != y)
        {
            previousMousePolledPoint = new PointF(x, y);
            GetWindowPosition(Surface.WindowHandle, out var posX, out var posY).LogErrorIfFailed();

            float rx = x - posX;
            float ry = y - posY;
            var vector = new Vector2(rx, ry);
            GlobalInputHandler.HandleMouseMove(vector);
        }


        // MouseButtonFlags buttonsToRelease = pressed & (globalButtons ^ pressed);
        // MouseButtonFlags buttonsToRelease = pressed & ~globalButtons;
        MouseButtonFlags buttonsToRelease = (pressed & ~globalButtons) & valid_buttons_mask;
        if (buttonsToRelease != 0)
        {
            Interlocked.And(ref pressedMouseButtons, (uint)~buttonsToRelease);

            Logger.Verbose($"Releasing via mouse poll (buttonsToRelease: {buttonsToRelease})");

            if (buttonsToRelease.HasFlagFast(MouseButtonFlags.Left)) GlobalInputHandler.HandleMouseButton(MouseButton.Left, false);
            if (buttonsToRelease.HasFlagFast(MouseButtonFlags.Middle)) GlobalInputHandler.HandleMouseButton(MouseButton.Middle, false);
            if (buttonsToRelease.HasFlagFast(MouseButtonFlags.Right)) GlobalInputHandler.HandleMouseButton(MouseButton.Right, false);
            if (buttonsToRelease.HasFlagFast(MouseButtonFlags.X1)) GlobalInputHandler.HandleMouseButton(MouseButton.Button1, false);
            if (buttonsToRelease.HasFlagFast(MouseButtonFlags.X2)) GlobalInputHandler.HandleMouseButton(MouseButton.Button2, false);
        }
    }

    protected void HandleEvent(Event sdlEvent)
    {
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
                GlobalInputHandler.HandleKeyboardInput(sdlEvent.Key);
                break;

            case EventType.TextEditing:
                GlobalInputHandler.HandleTextEditing(sdlEvent.Edit);
                break;

            case EventType.TextInput:
                GlobalInputHandler.HandleTextInput(sdlEvent.Text);
                break;

            case EventType.KeymapChanged:
                GlobalInputHandler.HandleKeymapChange();
                break;

            case EventType.FingerDown:
            case EventType.FingerUp:
            case EventType.FingerMotion:
            case EventType.FingerCanceled:
                GlobalInputHandler.HandleTouchInput(sdlEvent.TFinger);
                break;

            case EventType.DropBegin:
            case EventType.DropComplete:
            case EventType.DropFile:
            case EventType.DropPosition:
            case EventType.DropText:
                GlobalInputHandler.HandleDrop(sdlEvent.Drop);
                break;

            case EventType.PenProximityIn:
            case EventType.PenProximityOut:
                GlobalInputHandler.HandlePenProximity(sdlEvent.PProximity);
                break;

            case EventType.PenDown:
            case EventType.PenUp:
                GlobalInputHandler.HandlePenTouch(sdlEvent.PTouch);
                break;

            case EventType.PenMotion:
                GlobalInputHandler.HandlePenMotion(new Vector2(sdlEvent.PMotion.X, sdlEvent.PMotion.Y));
                break;

            case EventType.PenButtonUp:
            case EventType.PenButtonDown:
                GlobalInputHandler.HandlePenButton(sdlEvent.PButton);
                break;

            case EventType.MouseButtonDown:
            case EventType.MouseButtonUp:
                handleInternalMouseButtonEvent(sdlEvent.Button);
                break;

            case EventType.MouseMotion:
                handleInternalMouseMotionEvent(sdlEvent.Motion);
                break;
        }
    }

    private void handleInternalMouseButtonEvent(MouseButtonEvent mouseButtonEvent)
    {
        var mouseButton = mouseButtonEvent.ToMouseButton();
        var mask = ButtonMask(mouseButtonEvent.Button);

        switch (mouseButtonEvent.Type)
        {
            case EventType.MouseButtonDown:
                GlobalInputHandler.HandleMouseButton(mouseButton, true);
                Interlocked.And(ref pressedMouseButtons, mask);
                break;
            case EventType.MouseButtonUp:
                GlobalInputHandler.HandleMouseButton(mouseButton, false);
                Interlocked.And(ref pressedMouseButtons, ~mask);
                break;
        }
    }

    private void handleInternalMouseMotionEvent(MouseMotionEvent mouseMotionEvent)
    {
        if (GetWindowRelativeMouseMode(Surface.WindowHandle))
        {
            GlobalInputHandler.HandleMouseMove(new Vector2(mouseMotionEvent.X, mouseMotionEvent.Y));
        }
        else
        {
            GlobalInputHandler.HandleMouseMoveRelative(new Vector2(mouseMotionEvent.XRel, mouseMotionEvent.YRel));
        }
    }

    public bool CapsLockPressed => GetModState().HasFlagFast(Keymod.Caps);

    public bool AltPressed => GetModState().HasFlagFast(Keymod.Alt);

    public bool HasKeyboard => HasKeyboard();
}
