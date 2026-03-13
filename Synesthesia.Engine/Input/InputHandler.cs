// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Numerics;
using Faster.Map.Core;
using Synesthesia.Engine.Events;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform;
using Synesthesia.Engine.Util.Pooling;

namespace Synesthesia.Engine.Input;

public sealed class InputHandler : IFrameProcessor, IDisposable
{
    public static readonly FastObjectPool<KeyboardInputEvent> KEYBOARD_EVENT_POOL = new(() => new KeyboardInputEvent());
    public static readonly FastObjectPool<MouseButtonInputEvent> MOUSE_BUTTON_EVENT_POOL = new(() => new MouseButtonInputEvent());
    public static readonly FastObjectPool<MouseMoveInputEvent> MOUSE_MOVE_EVENT_POOL = new(() => new MouseMoveInputEvent());
    public static readonly FastObjectPool<MouseScrollInputEvent> MOUSE_SCROLL_INPUT_EVENT_POOL = new(() => new MouseScrollInputEvent());
    public static readonly FastObjectPool<TouchInputEvent> TOUCH_INPUT_EVENT_POOL = new(() => new TouchInputEvent());
    public static readonly FastObjectPool<TabletInputEvent> TABLET_INPUT_EVENT_POOL = new(() => new TabletInputEvent());

    public static readonly EventDispatcher<KeyboardInputEvent> ON_KEYBOARD_INPUT = new();
    public static readonly EventDispatcher<MouseButtonInputEvent> ON_MOUSE_BUTTON_INPUT = new();
    public static readonly EventDispatcher<MouseScrollInputEvent> ON_MOUSE_SCROLL_INPUT = new();
    public static readonly EventDispatcher<TouchInputEvent> ON_TOUCH_INPUT = new();

    private static readonly DenseMap<ulong, TouchInputEvent> active_touches = new();

    private static readonly List<Key> held_keys = [];
    private static readonly List<MouseButton> held_mouse_buttons = [];

    private readonly ConcurrentQueue<IInputEvent> eventQueue = new();

    public static bool IsKeyDown(Key key) => held_keys.Contains(key);

    public static bool IsMouseButtonDown(MouseButton mouseButton) => held_mouse_buttons.Contains(mouseButton);

    public static TouchInputEvent? GetTouchFingerInput(ulong fingerId) => active_touches.GetOrNullClass(fingerId);

    public static bool IsFingerDown(ulong fingerId) => GetTouchFingerInput(fingerId) != null;

    public void Enqueue(IInputEvent inputEvent) => eventQueue.Enqueue(inputEvent);

    public static Vector2 MousePosition = Vector2.Zero;

    public void ProcessFrame()
    {
        while (!eventQueue.IsEmpty)
        {
            if (!eventQueue.TryDequeue(out var inputEvent)) break;
            try
            {
                switch (inputEvent)
                {
                    case KeyboardInputEvent keyboardInputEvent:
                        handleKeyboardInput(keyboardInputEvent);
                        break;
                    case MouseButtonInputEvent mouseButtonInputEvent:
                        handleMouseButton(mouseButtonInputEvent);
                        break;
                    case TouchInputEvent touchInputEvent:
                        handleTouchInputEvent(touchInputEvent);
                        break;
                    // Mouse and Tablet events
                    case IPositionalInputEvent positionalInputEvent:
                        handlePositionalInputEvent(positionalInputEvent);
                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.Exception(exception, Logger.Input);
                throw;
            }
            finally
            {
                inputEvent.ReturnToPool();
            }
        }
    }
    private void handlePositionalInputEvent(IPositionalInputEvent positionalInputEvent)
    {
        // var isDelta = positionalInputEvent.PositionDelta != Vector2.Zero;
        //TODO Mose Delta
        MousePosition = positionalInputEvent.Position;
    }

    private void handleKeyboardInput(KeyboardInputEvent keyboardInputEvent)
    {
        var key = keyboardInputEvent.Key;
        if (keyboardInputEvent.IsDown)
        {
            held_keys.AddIfNotPresent(key);
        }
        else
        {
            if (!held_keys.Contains(key)) return;
            held_keys.Remove(key);
        }

        ON_KEYBOARD_INPUT.Dispatch(keyboardInputEvent);
    }

    private void handleMouseButton(MouseButtonInputEvent mouseButtonInputEvent)
    {
        var button = mouseButtonInputEvent.Button;
        if (mouseButtonInputEvent.IsDown)
        {
            if (held_mouse_buttons.Contains(button)) return;
            held_mouse_buttons.Add(button);
        }
        else
        {
            if (!held_mouse_buttons.Contains(button)) return;
            held_mouse_buttons.Remove(button);
        }
    }

    private void handleTouchInputEvent(TouchInputEvent touchInputEvent)
    {
        var finger = touchInputEvent.FingerId;
        if (touchInputEvent.IsDown)
        {
            if (active_touches.Contains(finger)) return;
            active_touches.Insert(finger, touchInputEvent);
        }
        else
        {
            if (!active_touches.Contains(finger)) return;
            active_touches.Remove(finger);
        }
    }

    // private void handleTextEditing(SDL.TextEditingEvent textEditingEvent)
    // {
    //     var text = textEditingEvent.Text;
    //     Logger.Verbose($"Text editing - {text}");
    // }
    //
    // private void handleTextInput(SDL.TextInputEvent textInputEvent)
    // {
    //     var text = textInputEvent.Text;
    //     Logger.Verbose($"Text input - {text}");
    // }

    public void Dispose()
    {
        ON_KEYBOARD_INPUT.Dispose();
        ON_MOUSE_BUTTON_INPUT.Dispose();
        ON_MOUSE_SCROLL_INPUT.Dispose();
        ON_TOUCH_INPUT.Dispose();
        eventQueue.Clear();
    }
}
