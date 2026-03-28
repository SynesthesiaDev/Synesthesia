// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Numerics;
using Faster.Map.Core;
using Synesthesia.Engine.Events;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Input.ActionBindings;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform;
using Synesthesia.Engine.Util.Pooling;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Input;

public sealed class InputHandler(Game game) : IFrameProcessor, IDisposable
{
    public static readonly FastObjectPool<KeyboardInputEvent> KEYBOARD_EVENT_POOL = new(() => new KeyboardInputEvent());
    public static readonly FastObjectPool<MouseButtonInputInputEvent> MOUSE_BUTTON_EVENT_POOL = new(() => new MouseButtonInputInputEvent());
    public static readonly FastObjectPool<MouseMoveInputEvent> MOUSE_MOVE_EVENT_POOL = new(() => new MouseMoveInputEvent());
    public static readonly FastObjectPool<MouseScrollInputEvent> MOUSE_SCROLL_INPUT_EVENT_POOL = new(() => new MouseScrollInputEvent());
    public static readonly FastObjectPool<TouchInputEvent> TOUCH_INPUT_EVENT_POOL = new(() => new TouchInputEvent());
    public static readonly FastObjectPool<TabletInputEvent> TABLET_INPUT_EVENT_POOL = new(() => new TabletInputEvent());

    public static readonly EventDispatcher<KeyboardInputEvent> ON_KEYBOARD_INPUT = new();
    public static readonly EventDispatcher<MouseButtonInputInputEvent> ON_MOUSE_BUTTON_INPUT = new();
    public static readonly EventDispatcher<MouseScrollInputEvent> ON_MOUSE_SCROLL_INPUT = new();
    public static readonly EventDispatcher<TouchInputEvent> ON_TOUCH_INPUT = new();

    private static readonly DenseMap<ulong, TouchInputEvent> active_touches = new();

    private static readonly List<Key> held_keys = [];
    private static readonly List<MouseButton> held_mouse_buttons = [];
    private static readonly List<PlatformActionBinding> held_action_bindings = [];

    private readonly ConcurrentQueue<IInputEvent> eventQueue = new();

    private static readonly List<PlatformActionBinding> platformActionBindings = [];

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
                    case MouseButtonInputInputEvent mouseButtonInputEvent:
                        handleMouseButton(mouseButtonInputEvent);
                        break;
                    case TouchInputEvent touchInputEvent:
                        handleTouchInputEvent(touchInputEvent);
                        break;
                    // Mouse AND Tablet events
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

        updateActionBindings();
    }

    public static void RegisterActionBinding(PlatformActionBinding binding)
    {
        var comparer = new ActionBindingComparer();

        foreach (var conflict in platformActionBindings
                     .Select(existing => binding.AlternativeBindings
                         .FirstOrDefault(newB => existing.AlternativeBindings
                             .Any(extB => comparer.Equals(newB, extB))))
                     .OfType<IActionBinding>())
        {
            throw new InvalidOperationException($"Cannot register a binding that overlaps with another binding ({conflict} is already bound)");
        }

        platformActionBindings.Add(binding);
    }

    // man I should really write tests for this but I don't feel like it
    private void updateActionBindings()
    {
        var consumedTriggers = new List<IActionBinding>();

        foreach (var action in platformActionBindings.OrderByDescending(a => a.Complexity))
        {
            var activeTrigger = action.AlternativeBindings.FirstOrDefault(b => b.IsDown);

            if (activeTrigger != null)
            {
                // check if the physical binding was used by another action binding
                // Ex. Pressing CTRL + F1 should not trigger just F1 binding
                bool isAlreadyUsed = consumedTriggers.Exists(t => isBindingComponentOf(activeTrigger, t));

                if (!isAlreadyUsed)
                {
                    consumedTriggers.Add(activeTrigger);

                    if (!held_action_bindings.Contains(action))
                    {
                        held_action_bindings.Add(action);
                        game.DrawableScene2d.UpdatePlatformActionBindingState(action);
                    }
                    continue;
                }
            }

            // action is not pressed or is shadowed
            if (held_action_bindings.Contains(action))
            {
                game.DrawableScene2d.UpdatePlatformActionBindingState(action);
                held_action_bindings.Remove(action);
            }
        }
    }

    private static bool isBindingComponentOf(IActionBinding simple, IActionBinding complex)
    {
        if (complex is KeyboardActionBinding complexKey && simple is KeyboardActionBinding simpleKey)
        {
            return complexKey.Primary == simpleKey.Primary && simpleKey.Modifiers.All(m => complexKey.Modifiers.Contains(m));
        }
        return false;
    }

    private void handlePositionalInputEvent(IPositionalInputEvent positionalInputEvent)
    {
        if (positionalInputEvent is TabletInputEvent) EngineStatistics.Increment(EngineStatistics.Type.TabletEvents);
        // var isDelta = positionalInputEvent.PositionDelta != Vector2.Zero;
        //TODO Mose Delta
        MousePosition = positionalInputEvent.Position;
        game.DrawableScene2d.UpdateHoverState(positionalInputEvent);
        updateActionBindings();
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

        game.DrawableScene2d.UpdateKeyState(keyboardInputEvent);
        ON_KEYBOARD_INPUT.Dispatch(keyboardInputEvent);
        updateActionBindings();
    }

    private void handleMouseButton(MouseButtonInputInputEvent mouseButtonInputInputEvent)
    {
        var button = mouseButtonInputInputEvent.Button;
        if (mouseButtonInputInputEvent.IsDown)
        {
            if (held_mouse_buttons.Contains(button)) return;
            held_mouse_buttons.Add(button);
        }
        else
        {
            if (!held_mouse_buttons.Contains(button)) return;
            held_mouse_buttons.Remove(button);
        }

        game.DrawableScene2d.UpdateCursorInputState(mouseButtonInputInputEvent);
        updateActionBindings();
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

        game.DrawableScene2d.UpdateCursorInputState(touchInputEvent);
        updateActionBindings();
    }

    public void Dispose()
    {
        ON_KEYBOARD_INPUT.Dispose();
        ON_MOUSE_BUTTON_INPUT.Dispose();
        ON_MOUSE_SCROLL_INPUT.Dispose();
        ON_TOUCH_INPUT.Dispose();
        eventQueue.Clear();
    }
}
