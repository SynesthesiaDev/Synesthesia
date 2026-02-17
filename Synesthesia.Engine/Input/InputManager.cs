using System.Collections.Immutable;
using System.Numerics;
using Common.Event;
using Common.Logger;
using Common.Pooling;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Input.Events;

namespace Synesthesia.Engine.Input;

public static class InputManager
{
    private static readonly Queue<IInputEvent> event_queue = new();

    public static Vector2 LastMousePosition = new(x: 0, 0);

    public static Vector2 LastMousePositionDelta = new(x: 0, 0);

    private static readonly List<ActionBinding> action_bindings = [];

    private static readonly List<ActionBinding> held_action_bindings = [];

    private static readonly List<KeyboardKey> held_keys = [];

    private static readonly List<MouseButton> held_mouse_buttons = [];

    public static readonly EventDispatcher<KeyboardKey> ON_KEY_DOWN = new();

    public static readonly EventDispatcher<KeyboardKey> ON_KEY_UP = new();

    public static readonly EventDispatcher<Vector2> ON_MOUSE_MOVE = new();

    public static readonly EventDispatcher<Vector2> ON_MOUSE_MOVE_DELTA = new();

    public static readonly FastObjectPool<TextInputEvent> TEXT_INPUT_EVENT_POOL = new(() => new TextInputEvent());

    public static readonly FastObjectPool<KeyInputEvent> KEY_INPUT_EVENT_POOL = new(() => new KeyInputEvent());

    public static readonly FastObjectPool<MouseButtonInputEvent> MOUSE_BUTTON_INPUT_EVENT_POOL = new(() => new MouseButtonInputEvent());

    public static readonly FastObjectPool<MouseMoveInputEvent> MOUSE_MOVE_INPUT_EVENT_POOL = new(() => new MouseMoveInputEvent());

    public static readonly FastObjectPool<MouseScrollWheelInputEvent> MOUSE_SCROLL_WHEEL_INPUT_EVENT_POOL = new(() => new MouseScrollWheelInputEvent());

    public static void InvalidMousePosition()
    {
        LastMousePosition = Vector2.Zero;
    }

    public static Vector2 MousePosition { get; private set; } = Vector2.Zero;

    public static ImmutableList<KeyboardKey> HeldKeys => held_keys.ToImmutableList();

    public static ImmutableList<MouseButton> HeldMouseButtons => held_mouse_buttons.ToImmutableList();

    public static bool IsDown(MouseButton mouseButton) => held_mouse_buttons.Contains(mouseButton);

    public static bool IsDown(KeyboardKey keyboardKey) => held_keys.Contains(keyboardKey);

    public static ImmutableList<ActionBinding> ActionBindings => action_bindings.ToImmutableList();

    public static void RegisterActionInput(ActionBinding actionBinding)
    {
        if (action_bindings.Contains(actionBinding) || action_bindings.Any(b => b.ActionName == actionBinding.ActionName))
        {
            var message = $"Action {actionBinding.ActionName} is already registered!";
            Logger.Error(message, Logger.Input);
            throw new InvalidOperationException(message);
        }

        action_bindings.Add(actionBinding);
    }

    private static IAcceptsFocus? focusedDrawable;

    public static IAcceptsFocus? FocusedDrawable
    {
        get => focusedDrawable;
        set
        {
            if (focusedDrawable == value) return;
            if (focusedDrawable != null)
            {
                focusedDrawable.OnFocusLost();
                Logger.Verbose($"Focus lost => {focusedDrawable.ObjectName()}", Logger.Input);
            }

            if (value != null)
            {
                value.OnFocusGained();
                Logger.Verbose($"Focus gained => {value.ObjectName()}", Logger.Input);
            }

            focusedDrawable = value;
        }
    }

    public static void EnqueueEvent(IInputEvent inputEvent)
    {
        lock (event_queue) event_queue.Enqueue(inputEvent);
    }

    public static void ProcessQueue(Game game)
    {
        while (true)
        {
            IInputEvent? inputEvent;
            lock (event_queue)
            {
                if (!event_queue.TryDequeue(out inputEvent)) break;
            }

            switch (inputEvent)
            {
                case KeyInputEvent keyInputEvent:
                {
                    try
                    {
                        if (keyInputEvent.IsDown)
                        {
                            held_keys.Add(keyInputEvent.Key);
                            ON_KEY_DOWN.Dispatch(keyInputEvent.Key);
                        }
                        else
                        {
                            held_keys.Remove(keyInputEvent.Key);
                            ON_KEY_UP.Dispatch(keyInputEvent.Key);
                        }

                        game.EngineDebugOverlay.UpdateKeyState(keyInputEvent.Key, keyInputEvent.IsDown);
                        game.RootComposite2d.UpdateKeyState(keyInputEvent.Key, keyInputEvent.IsDown);
                    }
                    finally
                    {
                        if (keyInputEvent.IsPooled) KEY_INPUT_EVENT_POOL.Return(keyInputEvent);
                    }

                    break;
                }

                case MouseButtonInputEvent mouseButtonInputEvent:
                {
                    try
                    {
                        if (mouseButtonInputEvent.IsDown)
                        {
                            held_mouse_buttons.Add(mouseButtonInputEvent.Button);
                        }
                        else
                        {
                            held_mouse_buttons.Remove(mouseButtonInputEvent.Button);

                            if (FocusedDrawable != null && !FocusedDrawable
                                    .GetOwningDrawable()
                                    .Contains(MousePosition))
                            {
                                FocusedDrawable = null;
                            }
                        }

                        var mouseEvent = new Drawable2d.PointInput(mouseButtonInputEvent, MousePosition, mouseButtonInputEvent.IsDown);
                        game.EngineDebugOverlay.UpdatePointInputState(mouseEvent, mouseEvent.IsDown);
                        game.RootComposite2d.UpdatePointInputState(mouseEvent, mouseEvent.IsDown);
                    }
                    finally
                    {
                        if (mouseButtonInputEvent.IsPooled) MOUSE_BUTTON_INPUT_EVENT_POOL.Return(mouseButtonInputEvent);
                    }

                    break;
                }

                case MouseMoveInputEvent mouseMoveInputEvent:
                {
                    try
                    {
                        MousePosition = mouseMoveInputEvent.Position;

                        ON_MOUSE_MOVE.Dispatch(mouseMoveInputEvent.Position);
                        ON_MOUSE_MOVE_DELTA.Dispatch(mouseMoveInputEvent.PositionDelta);

                        game.EngineDebugOverlay.UpdateHoverState(mouseMoveInputEvent);
                        game.RootComposite2d.UpdateHoverState(mouseMoveInputEvent);
                    }
                    finally
                    {
                        if (mouseMoveInputEvent.IsPooled) MOUSE_MOVE_INPUT_EVENT_POOL.Return(mouseMoveInputEvent);
                    }

                    break;
                }

                case MouseScrollWheelInputEvent mouseWheelInputEvent:
                {
                    try
                    {
                        game.EngineDebugOverlay.UpdateScrollWheelState(mouseWheelInputEvent);
                        game.RootComposite2d.UpdateScrollWheelState(mouseWheelInputEvent);
                    }
                    finally
                    {
                        if (mouseWheelInputEvent.IsPooled) MOUSE_SCROLL_WHEEL_INPUT_EVENT_POOL.Return(mouseWheelInputEvent);
                    }

                    break;
                }

                case TextInputEvent textInputEvent:
                {
                    try
                    {
                        FocusedDrawable?.OnCharacterTyped(textInputEvent.Character);
                    }
                    finally
                    {
                        if (textInputEvent.IsPooled) TEXT_INPUT_EVENT_POOL.Return(textInputEvent);
                    }

                    break;
                }
            }

            for (int i = 0; i < action_bindings.Count; i++)
            {
                var binding = action_bindings[i];

                var lastState = held_action_bindings.Contains(binding);
                var currentState = binding.IsDown;

                if (lastState == currentState) continue;

                if (currentState)
                {
                    game.EngineDebugOverlay.OnActionBindingDown(binding);
                }
                else
                {
                    game.EngineDebugOverlay.OnActionBindingUp(binding);
                }
            }
        }
    }
}
