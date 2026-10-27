// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Numerics;
using Faster.Map.Core;
using Synesthesia.Engine.Events;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Input.ActionBindings;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util.Pooling;
using System.Runtime.InteropServices;
using Synesthesia.Engine.Graphics.Three;
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
    public static readonly FastObjectPool<TextInputEvent> TEXT_INPUT_EVENT_POOL = new(() => new TextInputEvent());

    public static readonly EventDispatcher<KeyboardInputEvent> ON_KEYBOARD_INPUT = new();
    public static readonly EventDispatcher<MouseButtonInputInputEvent> ON_MOUSE_BUTTON_INPUT = new();
    public static readonly EventDispatcher<MouseScrollInputEvent> ON_MOUSE_SCROLL_INPUT = new();
    public static readonly EventDispatcher<TouchInputEvent> ON_TOUCH_INPUT = new();


    private static readonly List<Key> held_keys = [];
    private static readonly List<MouseButton> held_mouse_buttons = [];
    private static readonly List<PlatformActionBinding> held_action_bindings = [];

    private readonly ConcurrentQueue<IInputEvent> eventQueue = new();
    private static readonly List<PlatformActionBinding> platform_action_bindings = [];
    private static readonly DenseMap<ulong, TouchInputEvent> active_touches = new();

    private static Vector2 accumulatedMouseDelta = Vector2.Zero;
    private static Vector2 lastMousePosition = Vector2.Zero;
    private static readonly Lock mouse_delta_lock = new();

    public static bool IsKeyDown(Key key) => held_keys.Contains(key);

    public static bool IsMouseButtonDown(MouseButton mouseButton) => held_mouse_buttons.Contains(mouseButton);

    public static TouchInputEvent? GetTouchFingerInput(ulong fingerId) => active_touches.GetOrNullClass(fingerId);

    public static bool IsFingerDown(ulong fingerId) => GetTouchFingerInput(fingerId) != null;

    public void Enqueue(IInputEvent inputEvent) => eventQueue.Enqueue(inputEvent);

    public static Vector2 MousePosition = Vector2.Zero;

    private readonly InternalGameContainer2D gameContainer = game.GetInternalGameContainer();
    private readonly DrawableScene3D scene3d = game.DrawableScene3D;

    public static IAcceptsFocus? FocusedDrawable
    {
        get;
        set
        {
            if (field == value) return;
            if (field != null)
            {
                field.OnFocusLost();
                Logger.Verbose($"Focus lost => {field.GetType().Name}", Logger.Input);
            }

            if (value != null)
            {
                value.OnFocusGained();
                Logger.Verbose($"Focus gained => {value.GetType().Name}", Logger.Input);
            }

            field = value;
        }
    } = null;

    public void ProcessFrame(FrameInfo frameInfo)
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
                    case TextInputEvent textInputEvent:
                    {
                        handleTextInputEvent(textInputEvent);
                        break;
                    }
                    case MouseScrollInputEvent scrollInputEvent:
                    {
                        handleScrollWheelEvent(scrollInputEvent);
                        break;
                    }
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

        // updateActionBindings();
    }

    public static void RegisterActionBinding(PlatformActionBinding binding)
    {
        var comparer = new ActionBindingComparer();

        foreach (var conflict in platform_action_bindings
                     .Select(existing => binding.AlternativeBindings
                         .FirstOrDefault(newB => existing.AlternativeBindings
                             .Any(extB => comparer.Equals(newB, extB))))
                     .OfType<IActionBinding>())
        {
            throw new InvalidOperationException($"Cannot register a binding that overlaps with another binding ({conflict} is already bound)");
        }

        platform_action_bindings.Add(binding);
    }

    private readonly List<IActionBinding> consumedTriggers = [];

    private static readonly Comparison<PlatformActionBinding> complexity_comparer =
        (a, b) => b.Complexity.CompareTo(a.Complexity);

    // man I should really write tests for this, but I don't feel like it
    private void updateActionBindings()
    {
        consumedTriggers.Clear();

        platform_action_bindings.Sort(complexity_comparer);

        foreach (ref PlatformActionBinding action in CollectionsMarshal.AsSpan(platform_action_bindings))
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
                        gameContainer.UpdatePlatformActionBindingState(action);
                        scene3d.UpdatePlatformActionBindingState(action);
                    }

                    continue;
                }
            }

            // action is not pressed or is shadowed
            if (held_action_bindings.Contains(action))
            {
                gameContainer.UpdatePlatformActionBindingState(action);
                scene3d.UpdatePlatformActionBindingState(action);
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

    private void handleTextInputEvent(TextInputEvent textInputEvent)
    {
        if (textInputEvent.Text == string.Empty) return;
        FocusedDrawable?.OnTextTyped(textInputEvent.Text);
    }

    private void handleScrollWheelEvent(MouseScrollInputEvent scrollInputEvent)
    {
        gameContainer.UpdateScrollWheelState(scrollInputEvent);
        scene3d.UpdateScrollWheelState(scrollInputEvent);
    }

    public static Vector2 ConsumeMouseDelta()
    {
        lock (mouse_delta_lock)
        {
            var delta = accumulatedMouseDelta;
            accumulatedMouseDelta = Vector2.Zero;
            return delta;
        }
    }

    private void handlePositionalInputEvent(IPositionalInputEvent positionalInputEvent)
    {
        if (positionalInputEvent is TabletInputEvent) EngineStatistics.Increment(EngineStatistics.Type.TabletEvents);

        if (positionalInputEvent.PositionDelta != Vector2.Zero)
        {
            lock (mouse_delta_lock)
            {
                accumulatedMouseDelta += positionalInputEvent.PositionDelta;
            }
        }
        else
        {
            // 2. Normal absolute mouse movement (UI hovering)
            var currentPos = positionalInputEvent.Position;
            if (lastMousePosition != Vector2.Zero)
            {
                float dx = currentPos.X - lastMousePosition.X;
                float dy = currentPos.Y - lastMousePosition.Y;

                lock (mouse_delta_lock)
                {
                    accumulatedMouseDelta += new Vector2(dx, dy);
                }
            }
            lastMousePosition = currentPos;
        }

        MousePosition = positionalInputEvent.Position;
        gameContainer.UpdateHoverState(positionalInputEvent);
        updateActionBindings();
    }

    public void SetMouseCaptured(bool captured)
    {
        lock (mouse_delta_lock)
        {
            Logger.Verbose("reset mouse delta");
            accumulatedMouseDelta = Vector2.Zero;
            lastMousePosition = Vector2.Zero;
        }
        game.WindowHost.SetMouseCaptured(captured);
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

        gameContainer.UpdateKeyState(keyboardInputEvent);
        scene3d.UpdateKeyState(keyboardInputEvent);

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
            tryInvalidateFocusedDrawableFocus();
        }

        gameContainer.UpdateCursorInputState(mouseButtonInputInputEvent);
        scene3d.UpdateCursorInputState(mouseButtonInputInputEvent);
        updateActionBindings();
    }

    private void tryInvalidateFocusedDrawableFocus()
    {
        if (FocusedDrawable != null && !FocusedDrawable
                .OwningDrawable
                .Contains(MousePosition))
        {
            FocusedDrawable = null;
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
            tryInvalidateFocusedDrawableFocus();
        }

        gameContainer.UpdateCursorInputState(touchInputEvent);
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
