using System.Collections.Immutable;
using System.Numerics;
using Common.Logger;
using Raylib_cs;
using Synesthesia.Engine.Graphics.Two;

namespace Synesthesia.Engine.Input;

public static class InputManager
{
    private static readonly Queue<IInputEvent> EventQueue = new();
    private static Vector2 _lastMousePos = new(x: 0, 0);
    private static readonly Dictionary<(InputSource source, int id), bool> LastStates = new();

    private static readonly List<ActionBinding> _actionBindings = [];

    public static ImmutableList<ActionBinding> ActionBindings => _actionBindings.ToImmutableList();

    public static void RegisterActionInput(ActionBinding actionBinding)
    {
        if (_actionBindings.Contains(actionBinding) || _actionBindings.Any(b => b.ActionName == actionBinding.ActionName))
        {
            var message = $"Action {actionBinding.ActionName} is already registered!";
            Logger.Error(message);
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
            _focusedDrawable?.OnFocusLost();
            value?.OnFocusGained();
            _focusedDrawable = value;
        }
    }

    private record InputEventData(KeyboardKey Key, bool IsDown);

    public static void EnqueueEvent(IInputEvent inputEvent)
    {
        lock (EventQueue) EventQueue.Enqueue(inputEvent);
    }

    public static void PollInputs(Game game)
    {
        while (true)
        {
            IInputEvent? inputEvent;
            lock (EventQueue)
            {
                if (!EventQueue.TryDequeue(out inputEvent)) break;
            }
            
            foreach (var binding in _actionBindings)
            {
                if (MatchesBinding(binding, inputEvent))
                {
                    game.RootComposite2d.UpdateActionBindingState(binding, true);
                    HandleActionBinding(binding);
                }
                else if (IsReleaseEvent(binding, inputEvent))
                {
                    game.RootComposite2d.UpdateActionBindingState(binding, false);
                }
            }
        }
    }
    
    private static bool MatchesBinding(ActionBinding binding, IInputEvent inputEvent)
    {
        return inputEvent switch
        {
            KeyInputEvent keyEvent when binding.Key != null => 
                keyEvent.Key == binding.Key.Key && 
                keyEvent.IsDown && 
                binding.Key.Modifiers.All(p => Raylib.IsKeyDown(p)),

            MouseInputEvent mouseEvent when binding.MouseButton != null => 
                mouseEvent.Button == binding.MouseButton.Key && 
                mouseEvent.IsDown && 
                binding.MouseButton.Modifiers.All(p => Raylib.IsMouseButtonDown(p)),

            TouchInputEvent touchEvent when binding.TouchGesture != null => 
                touchEvent.Gesture == binding.TouchGesture.Key && 
                touchEvent.IsDown, // Gestures typically don't have "held" modifiers in the same way

            _ => false
        };
    }
    
    private static bool IsReleaseEvent(ActionBinding binding, IInputEvent inputEvent)
    {
        return inputEvent switch
        {
            KeyInputEvent keyEvent => binding.Key?.Key == keyEvent.Key && !keyEvent.IsDown,
            MouseInputEvent mouseEvent => binding.MouseButton?.Key == mouseEvent.Button && !mouseEvent.IsDown,
            _ => false
        };
    }

    public static void HandleActionBinding(ActionBinding actionBinding)
    {
        
        //will do this myself later
    }

    private static void HandlePointInput(Game game, Vector2 position, InputSource source)
    {
        var moved = position != _lastMousePos;
        _lastMousePos = position;

        if (moved)
        {
            var hoverEvent = new Drawable2d.HoverEvent(true, position);
            game.EngineDebugOverlay.UpdateHoverState(hoverEvent);
            game.RootComposite2d.UpdateHoverState(hoverEvent);
        }

        var buttonCount = (source == InputSource.Mouse) ? 6 : 1;
        for (var i = 0; i < buttonCount; i++)
        {
            bool isDown = source switch
            {
                InputSource.Keyboard => Raylib.IsKeyDown((KeyboardKey)i),
                InputSource.Mouse => Raylib.IsMouseButtonDown((MouseButton)i),
                InputSource.Touch => Raylib.IsGestureDetected((Gesture)i),
                _ => false
            };

            var key = (source, i);
            LastStates.TryGetValue(key, out var wasDown);

            if (isDown == wasDown) continue;
            LastStates[key] = isDown;

            if (!isDown && FocusedDrawable != null && !FocusedDrawable.GetOwningDrawable().Contains(position))
            {
                FocusedDrawable = null;
            }

            IInputEvent inputEvent = source switch
            {
                InputSource.Keyboard => new KeyInputEvent((KeyboardKey)i, isDown),
                InputSource.Mouse => new MouseInputEvent((MouseButton)i, position, isDown),
                InputSource.Touch => new TouchInputEvent((Gesture)i, position, isDown),
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };

            var mouseEvent = new Drawable2d.PointInput(inputEvent, position, isDown);
            game.EngineDebugOverlay.UpdatePointInputState(mouseEvent, isDown);
            game.RootComposite2d.UpdatePointInputState(mouseEvent, isDown);
        }
    }

    public static void PollMouse(Game game)
    {
        var mousePos = Raylib.GetMousePosition();
        HandlePointInput(game, mousePos, InputSource.Mouse);
    }

    private class InputState
    {
        public readonly HashSet<KeyboardKey> PressedKeys = [];
    }
}