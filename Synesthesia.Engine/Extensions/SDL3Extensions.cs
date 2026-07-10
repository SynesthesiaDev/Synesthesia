// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.CompilerServices;
using SDL3;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform;
using Synesthesia.Engine.Util.Exceptions;
using SynesthesiaUtil;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Extensions;

public static class SDL3Extensions
{
    extension(bool returnValue)
    {
        public bool LogErrorIfFailed([CallerArgumentExpression("returnValue")] string? expression = null)
        {
            if (!returnValue) logError(expression);
            return returnValue;
        }

        public void ThrowIfFailed([CallerArgumentExpression("returnValue")] string? expression = null)
        {
            if (!returnValue) throwError(expression);
        }
    }

    public static SystemTheme ToSystemTheme(this SDL.SystemTheme sdlTheme)
    {
        return sdlTheme switch
        {
            SDL.SystemTheme.Unknown => SystemTheme.Unknown,
            SDL.SystemTheme.Light => SystemTheme.Light,
            SDL.SystemTheme.Dark => SystemTheme.Dark,
            _ => throw new ArgumentOutOfRangeException(nameof(sdlTheme), sdlTheme, null),
        };
    }

    public static Vector4 ToVector(this SDL.Rect rect) => new(rect.X, rect.Y, rect.H, rect.Y);

    public static int LogErrorIfFailed(this int returnValue, [CallerArgumentExpression("returnValue")] string? expression = null)
    {
        if (returnValue == -1) logError(expression);
        return returnValue;
    }

    private static void logError(string? expression)
    {
        Logger.Error($"SDL error: {SDL.GetError()}");
        if (!string.IsNullOrEmpty(expression))
            Logger.Error($"at {expression}");
    }

    private static void throwError(string? expression)
    {
        throw new SDLPlatformException("SDL Error", expression);
    }


    public static MouseButton ToMouseButton(this SDL.MouseButtonEvent mouseButtonEvent)
    {
        var button = mouseButtonEvent.Button;
        switch (button)
        {
            case SDL.ButtonLeft:
                return MouseButton.Left;

            case SDL.ButtonRight:
                return MouseButton.Right;

            case SDL.ButtonMiddle:
                return MouseButton.Middle;

            case SDL.ButtonX1:
                return MouseButton.Button1;

            case SDL.ButtonX2:
                return MouseButton.Button2;

            default:
                Logger.Warning($"Unknown SDL3 mouse button: {button}, defaulting to left button");
                return MouseButton.Left;
        }
    }

    public static Key ToKey(this SDL.KeyboardEvent sdlKeyboardEvent)
    {
        // Apple devices don't have the notion of NumLock (they have a Clear key instead).
        // treat them as if they always have NumLock on (the numpad always performs its primary actions).
        bool numLockOn = sdlKeyboardEvent.Mod.HasFlagFast(SDL.Keymod.Num) || RuntimeInfo.IsApple;

        switch (sdlKeyboardEvent.Scancode)
        {
            default:
            case SDL.Scancode.Unknown:
                return Key.Unknown;

            case SDL.Scancode.KpComma:
                return Key.Comma;

            case SDL.Scancode.KpTab:
                return Key.Tab;

            case SDL.Scancode.KpBackspace:
                return Key.BackSpace;

            case SDL.Scancode.KpA:
                return Key.A;

            case SDL.Scancode.KpB:
                return Key.B;

            case SDL.Scancode.KpC:
                return Key.C;

            case SDL.Scancode.KpD:
                return Key.D;

            case SDL.Scancode.KpE:
                return Key.E;

            case SDL.Scancode.KpF:
                return Key.F;

            case SDL.Scancode.KpSpace:
                return Key.Space;

            case SDL.Scancode.KpClear:
                return Key.Clear;

            case SDL.Scancode.Return:
                return Key.Enter;

            case SDL.Scancode.Escape:
                return Key.Escape;

            case SDL.Scancode.Backspace:
                return Key.BackSpace;

            case SDL.Scancode.Tab:
                return Key.Tab;

            case SDL.Scancode.Space:
                return Key.Space;

            case SDL.Scancode.Apostrophe:
                return Key.Quote;

            case SDL.Scancode.Comma:
                return Key.Comma;

            case SDL.Scancode.Minus:
                return Key.Minus;

            case SDL.Scancode.Period:
                return Key.Period;

            case SDL.Scancode.Slash:
                return Key.Slash;

            case SDL.Scancode.Alpha0:
                return Key.Number0;

            case SDL.Scancode.Alpha1:
                return Key.Number1;

            case SDL.Scancode.Alpha2:
                return Key.Number2;

            case SDL.Scancode.Alpha3:
                return Key.Number3;

            case SDL.Scancode.Alpha4:
                return Key.Number4;

            case SDL.Scancode.Alpha5:
                return Key.Number5;

            case SDL.Scancode.Alpha6:
                return Key.Number6;

            case SDL.Scancode.Alpha7:
                return Key.Number7;

            case SDL.Scancode.Alpha8:
                return Key.Number8;

            case SDL.Scancode.Alpha9:
                return Key.Number9;

            case SDL.Scancode.Semicolon:
                return Key.Semicolon;

            case SDL.Scancode.Equals:
                return Key.Plus;

            case SDL.Scancode.Leftbracket:
                return Key.BracketLeft;

            case SDL.Scancode.Backslash:
                return Key.BackSlash;

            case SDL.Scancode.Rightbracket:
                return Key.BracketRight;

            case SDL.Scancode.Grave:
                return Key.Tilde;

            case SDL.Scancode.A:
                return Key.A;

            case SDL.Scancode.B:
                return Key.B;

            case SDL.Scancode.C:
                return Key.C;

            case SDL.Scancode.D:
                return Key.D;

            case SDL.Scancode.E:
                return Key.E;

            case SDL.Scancode.F:
                return Key.F;

            case SDL.Scancode.G:
                return Key.G;

            case SDL.Scancode.H:
                return Key.H;

            case SDL.Scancode.I:
                return Key.I;

            case SDL.Scancode.J:
                return Key.J;

            case SDL.Scancode.K:
                return Key.K;

            case SDL.Scancode.L:
                return Key.L;

            case SDL.Scancode.M:
                return Key.M;

            case SDL.Scancode.N:
                return Key.N;

            case SDL.Scancode.O:
                return Key.O;

            case SDL.Scancode.P:
                return Key.P;

            case SDL.Scancode.Q:
                return Key.Q;

            case SDL.Scancode.R:
                return Key.R;

            case SDL.Scancode.S:
                return Key.S;

            case SDL.Scancode.T:
                return Key.T;

            case SDL.Scancode.U:
                return Key.U;

            case SDL.Scancode.V:
                return Key.V;

            case SDL.Scancode.W:
                return Key.W;

            case SDL.Scancode.X:
                return Key.X;

            case SDL.Scancode.Y:
                return Key.Y;

            case SDL.Scancode.Z:
                return Key.Z;

            case SDL.Scancode.Capslock:
                return Key.CapsLock;

            case SDL.Scancode.F1:
                return Key.F1;

            case SDL.Scancode.F2:
                return Key.F2;

            case SDL.Scancode.F3:
                return Key.F3;

            case SDL.Scancode.F4:
                return Key.F4;

            case SDL.Scancode.F5:
                return Key.F5;

            case SDL.Scancode.F6:
                return Key.F6;

            case SDL.Scancode.F7:
                return Key.F7;

            case SDL.Scancode.F8:
                return Key.F8;

            case SDL.Scancode.F9:
                return Key.F9;

            case SDL.Scancode.F10:
                return Key.F10;

            case SDL.Scancode.F11:
                return Key.F11;

            case SDL.Scancode.F12:
                return Key.F12;

            case SDL.Scancode.Printscreen:
                return Key.PrintScreen;

            case SDL.Scancode.Scrolllock:
                return Key.ScrollLock;

            case SDL.Scancode.Pause:
                return Key.Pause;

            case SDL.Scancode.Insert:
                return Key.Insert;

            case SDL.Scancode.Home:
                return Key.Home;

            case SDL.Scancode.Pageup:
                return Key.PageUp;

            case SDL.Scancode.Delete:
                return Key.Delete;

            case SDL.Scancode.End:
                return Key.End;

            case SDL.Scancode.Pagedown:
                return Key.PageDown;

            case SDL.Scancode.Right:
                return Key.Right;

            case SDL.Scancode.Left:
                return Key.Left;

            case SDL.Scancode.Down:
                return Key.Down;

            case SDL.Scancode.Up:
                return Key.Up;

            case SDL.Scancode.NumLockClear:
                return Key.NumLock;

            case SDL.Scancode.KpDivide:
                return Key.KeypadDivide;

            case SDL.Scancode.KpMultiply:
                return Key.KeypadMultiply;

            case SDL.Scancode.KpMinus:
                return Key.KeypadMinus;

            case SDL.Scancode.KpPlus:
                return Key.KeypadPlus;

            case SDL.Scancode.KpEnter:
                return Key.KeypadEnter;

            case SDL.Scancode.Kp1:
                return numLockOn ? Key.Keypad1 : Key.End;

            case SDL.Scancode.Kp2:
                return numLockOn ? Key.Keypad2 : Key.Down;

            case SDL.Scancode.Kp3:
                return numLockOn ? Key.Keypad3 : Key.PageDown;

            case SDL.Scancode.Kp4:
                return numLockOn ? Key.Keypad4 : Key.Left;

            case SDL.Scancode.Kp5:
                return numLockOn ? Key.Keypad5 : Key.Clear;

            case SDL.Scancode.Kp6:
                return numLockOn ? Key.Keypad6 : Key.Right;

            case SDL.Scancode.Kp7:
                return numLockOn ? Key.Keypad7 : Key.Home;

            case SDL.Scancode.Kp8:
                return numLockOn ? Key.Keypad8 : Key.Up;

            case SDL.Scancode.Kp9:
                return numLockOn ? Key.Keypad9 : Key.PageUp;

            case SDL.Scancode.Kp0:
                return numLockOn ? Key.Keypad0 : Key.Insert;

            case SDL.Scancode.KpPeriod:
                return numLockOn ? Key.KeypadPeriod : Key.Delete;

            case SDL.Scancode.NonUsBackSlash:
                return Key.NonUsBackSlash;

            case SDL.Scancode.F13:
                return Key.F13;

            case SDL.Scancode.F14:
                return Key.F14;

            case SDL.Scancode.F15:
                return Key.F15;

            case SDL.Scancode.F16:
                return Key.F16;

            case SDL.Scancode.F17:
                return Key.F17;

            case SDL.Scancode.F18:
                return Key.F18;

            case SDL.Scancode.F19:
                return Key.F19;

            case SDL.Scancode.F20:
                return Key.F20;

            case SDL.Scancode.F21:
                return Key.F21;

            case SDL.Scancode.F22:
                return Key.F22;

            case SDL.Scancode.F23:
                return Key.F23;

            case SDL.Scancode.F24:
                return Key.F24;

            case SDL.Scancode.Menu:
            case SDL.Scancode.Application:
                return Key.Menu;

            case SDL.Scancode.Stop:
                return Key.Stop;

            case SDL.Scancode.Mute:
                return Key.Mute;

            case SDL.Scancode.VolumeUp:
                return Key.VolumeUp;

            case SDL.Scancode.VolumeDown:
                return Key.VolumeDown;

            case SDL.Scancode.Clear:
                return Key.Clear;

            case SDL.Scancode.DecimalSeparator:
                return Key.KeypadDecimal;

            case SDL.Scancode.LCtrl:
                return Key.ControlLeft;

            case SDL.Scancode.LShift:
                return Key.ShiftLeft;

            case SDL.Scancode.LAlt:
                return Key.AltLeft;

            case SDL.Scancode.LGUI:
                return Key.WinLeft;

            case SDL.Scancode.RCtrl:
                return Key.ControlRight;

            case SDL.Scancode.RShift:
                return Key.ShiftRight;

            case SDL.Scancode.RAlt:
                return Key.AltRight;

            case SDL.Scancode.RGUI:
                return Key.WinRight;

            case SDL.Scancode.MediaNextTrack:
                return Key.TrackNext;

            case SDL.Scancode.MediaPreviousTrack:
                return Key.TrackPrevious;

            case SDL.Scancode.MediaStop:
                return Key.Stop;

            case SDL.Scancode.MediaPlayPause:
                return Key.PlayPause;

            case SDL.Scancode.Sleep:
                return Key.Sleep;

            case SDL.Scancode.ACBack:
                return Key.Escape;
        }
    }
}
