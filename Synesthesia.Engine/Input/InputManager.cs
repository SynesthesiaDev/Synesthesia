using System.Numerics;
using Common.Event;
using Common.Logger;
using Raylib_cs;
using Synesthesia.Engine.Graphics.Two;

namespace Synesthesia.Engine.Input;

public static class InputManager
{
    private static List<HotKey> _registeredHotKeys = [];
    private static readonly Queue<InputEventData> _pendingEvents = new();

    public static EventDispatcher<InputEvent> KeyDown = new();
    public static EventDispatcher<InputEvent> KeyUp = new();
    public static EventDispatcher<InputEvent> Press = new();
    private static readonly InputState _currentState = new();

    private static Vector2 _lastMousePos = new Vector2(0, 0);
    private static readonly bool[] _lastMouseState = new bool[6];

    private record InputEventData(KeyboardKey Key, bool IsDown);

    public static void Register(HotKey hotKey)
    {
        _registeredHotKeys.Add(hotKey);
    }

    public record InputEvent(HotKey HotKey)
    {
    }

    public static void EnqueueKeyEvent(KeyboardKey key, bool isDown)
    {
        lock (_pendingEvents)
        {
            _pendingEvents.Enqueue(new InputEventData(key, isDown));
        }
    }

    public static void PollInputs()
    {
        while (true)
        {
            InputEventData? data;
            lock (_pendingEvents)
            {
                if (!_pendingEvents.TryDequeue(out data)) break;
            }

            if (data.IsDown) _currentState.PressedKeys.Add(data.Key);
            else _currentState.PressedKeys.Remove(data.Key);

            foreach (var ev
                     in from hotKey in _registeredHotKeys
                     where data.Key == hotKey.Key
                     where hotKey.Modifiers.All(m => _currentState.PressedKeys.Contains(m))
                     select new InputEvent(hotKey))
            {
                if (data.IsDown)
                {
                    Press.Dispatch(ev);
                    KeyDown.Dispatch(ev);
                }
                else
                {
                    KeyUp.Dispatch(ev);
                }
            }
        }
    }

    public static void PollMouse(Game game)
    {
        var mousePos = Raylib.GetMousePosition();
        var moved = mousePos != _lastMousePos;
        _lastMousePos = mousePos;

        var hoverEvent = new Drawable2d.HoverEvent(true, mousePos);

        if (moved)
        {
            game.EngineDebugOverlay.UpdateHoverState(hoverEvent);
            game.RootComposite2d.UpdateHoverState(hoverEvent);
        }

        for (var i = 0; i < 6; i++)
        {
            var mouseButton = (MouseButton)i;
            var isDown = Raylib.IsMouseButtonDown(mouseButton);
            var wasDown = _lastMouseState[i];

            if (isDown == wasDown) continue;

            var mouseEvent = new Drawable2d.MouseEvent(mouseButton, mousePos);
            game.EngineDebugOverlay.UpdateMouseClickState(mouseEvent, isDown);
            game.RootComposite2d.UpdateMouseClickState(mouseEvent, isDown);

            _lastMouseState[i] = isDown;
        }
    }

    private class InputState
    {
        public readonly HashSet<KeyboardKey> PressedKeys = [];
    }
}