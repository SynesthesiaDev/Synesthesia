using System.Collections.Immutable;
using System.Numerics;
using Common.Logger;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Two;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Input;

public static class InputManager
{
    private static readonly Queue<IInputEvent> event_queue = new();
    
    public static Vector2 LastMousePosition = new(x: 0, 0);

    private static readonly List<ActionBinding> action_bindings = [];

    private static readonly List<ActionBinding> held_action_bindings = [];

    private static readonly List<KeyboardKey> held_keys = [];

    private static readonly List<MouseButton> held_mouse_buttons = [];

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
                    if (keyInputEvent.IsDown)
                    {
                        held_keys.Add(keyInputEvent.Key);
                    }
                    else
                    {
                        held_keys.Remove(keyInputEvent.Key);
                    }
                    
                    game.EngineDebugOverlay.UpdateKeyState(keyInputEvent.Key, keyInputEvent.IsDown);
                    game.RootComposite2d.UpdateKeyState(keyInputEvent.Key, keyInputEvent.IsDown);
                    break;
                }

                case MouseButtonInputEvent mouseButtonInputEvent:
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
                    break;
                }

                case MouseMoveInputEvent mouseMoveInputEvent:
                {
                    MousePosition = mouseMoveInputEvent.Position;
                    var hoverEvent = new Drawable2d.HoverEvent(true, MousePosition);
                    game.EngineDebugOverlay.UpdateHoverState(hoverEvent);
                    game.RootComposite2d.UpdateHoverState(hoverEvent);
                    break;
                }

                case TextInputEvent textInputEvent:
                {
                    FocusedDrawable?.OnCharacterTyped(textInputEvent.Character);
                    break;
                }
            }

            action_bindings.ForEach(binding =>
            {
                var lastState = held_action_bindings.Contains(binding);
                var currentState = binding.IsDown;

                if (lastState != currentState)
                {
                    if (currentState)
                    {
                        game.EngineDebugOverlay.OnActionBindingDown(binding);
                    }
                    else
                    {
                        game.EngineDebugOverlay.OnActionBindingUp(binding);
                    }
                }
            });
        }
    }
}