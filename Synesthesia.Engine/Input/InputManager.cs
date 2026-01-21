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
    private static readonly Queue<IInputEvent> EventQueue = new();
    
    public static Vector2 LastMousePosition = new(x: 0, 0);

    private static readonly List<ActionBinding> _actionBindings = [];

    private static readonly List<ActionBinding> _heldActionBindings = [];

    private static readonly List<KeyboardKey> _heldKeys = [];

    private static readonly List<MouseButton> _heldMouseButtons = [];

    public static void InvalidMousePosition()
    {
        LastMousePosition = Vector2.Zero;
    }
    
    public static Vector2 MousePosition { get; private set; } = Vector2.Zero;

    public static ImmutableList<KeyboardKey> HeldKeys => _heldKeys.ToImmutableList();

    public static ImmutableList<MouseButton> HeldMouseButtons => _heldMouseButtons.ToImmutableList();

    public static bool IsDown(MouseButton mouseButton) => _heldMouseButtons.Contains(mouseButton);

    public static bool IsDown(KeyboardKey keyboardKey) => _heldKeys.Contains(keyboardKey);

    public static ImmutableList<ActionBinding> ActionBindings => _actionBindings.ToImmutableList();

    public static void RegisterActionInput(ActionBinding actionBinding)
    {
        if (_actionBindings.Contains(actionBinding) || _actionBindings.Any(b => b.ActionName == actionBinding.ActionName))
        {
            var message = $"Action {actionBinding.ActionName} is already registered!";
            Logger.Error(message, Logger.INPUT);
            throw new InvalidOperationException(message);
        }

        _actionBindings.Add(actionBinding);
    }

    private static IAcceptsFocus? _focusedDrawable;

    public static IAcceptsFocus? FocusedDrawable
    {
        get => _focusedDrawable;
        set
        {
            if (_focusedDrawable == value) return;
            if (_focusedDrawable != null)
            {
                _focusedDrawable.OnFocusLost();
                Logger.Verbose($"Focus lost => {_focusedDrawable.ObjectName()}", Logger.INPUT);
            }

            if (value != null)
            {
                value.OnFocusGained();
                Logger.Verbose($"Focus gained => {value.ObjectName()}", Logger.INPUT);
            }

            _focusedDrawable = value;
        }
    }

    public static void EnqueueEvent(IInputEvent inputEvent)
    {
        lock (EventQueue) EventQueue.Enqueue(inputEvent);
    }

    public static void ProcessQueue(Game game)
    {
        while (true)
        {
            IInputEvent? inputEvent;
            lock (EventQueue)
            {
                if (!EventQueue.TryDequeue(out inputEvent)) break;
            }

            switch (inputEvent)
            {
                case KeyInputEvent keyInputEvent:
                {
                    if (keyInputEvent.IsDown)
                    {
                        _heldKeys.Add(keyInputEvent.Key);
                    }
                    else
                    {
                        _heldKeys.Remove(keyInputEvent.Key);
                    }
                    
                    game.EngineDebugOverlay.UpdateKeyState(keyInputEvent.Key, keyInputEvent.IsDown);
                    game.RootComposite2d.UpdateKeyState(keyInputEvent.Key, keyInputEvent.IsDown);
                    break;
                }

                case MouseButtonInputEvent mouseButtonInputEvent:
                {
                    if (mouseButtonInputEvent.IsDown)
                    {
                        _heldMouseButtons.Add(mouseButtonInputEvent.Button);
                    }
                    else
                    {
                        _heldMouseButtons.Remove(mouseButtonInputEvent.Button);

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

            _actionBindings.ForEach(binding =>
            {
                var lastState = _heldActionBindings.Contains(binding);
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