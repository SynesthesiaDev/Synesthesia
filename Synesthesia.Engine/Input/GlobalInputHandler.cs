// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using SDL3;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Logging;

namespace Synesthesia.Engine.Input;

public static class GlobalInputHandler
{
    public static void HandleKeyboardInput(SDL.KeyboardEvent keyboardEvent)
    {
        var key = keyboardEvent.ToKey();
        if (keyboardEvent.Down)
        {
            Logger.Verbose($"Key Down - {key}");
        }
        else
        {
            Logger.Verbose($"Key Up - {key}");
        }
    }

    public static void HandleMouseButton(MouseButton mouseButton, bool down)
    {
        Logger.Verbose($"{mouseButton} down: {down}");
    }

    public static void HandleTextEditing(SDL.TextEditingEvent textEditingEvent)
    {
        var text = textEditingEvent.Text;
        Logger.Verbose($"Text editing - {text}");
    }

    public static void HandleTextInput(SDL.TextInputEvent textInputEvent)
    {
        var text = textInputEvent.Text;
        Logger.Verbose($"Text input - {text}");
    }

    public static void HandleTouchInput(SDL.TouchFingerEvent touchFingerEvent)
    {

    }

    public static void HandleDrop(SDL.DropEvent dropEvent)
    {

    }

    public static void HandlePenProximity(SDL.PenProximityEvent penProximityEvent)
    {
        Logger.Verbose($"Pen proximity - {penProximityEvent}!!");

    }

    public static void HandlePenTouch(SDL.PenTouchEvent penTouchEvent)
    {

    }

    public static void HandlePenButton(SDL.PenButtonEvent penButtonEvent)
    {
        Logger.Verbose($"Pen button - {penButtonEvent.Button}");
    }

    public static void HandlePenMotion(SDL.PenMotionEvent penMotionEvent)
    {
        Logger.Verbose($"Pen motion - {penMotionEvent.X} {penMotionEvent.Y}");
    }
    public static void HandleKeymapChange()
    {
        Logger.Verbose("Keymap change");
    }

    public static void HandleMouseMove(Vector2 newPosition)
    {
    }

    public static void HandleMouseMoveRelative(Vector2 newPosition)
    {

    }
}
