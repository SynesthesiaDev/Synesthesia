using Common.Event;
using Raylib_cs;

namespace Synesthesia.Engine.Input;

public static class InputManager
{
    private static List<HotKey> _registeredHotKeys = [];
    private static readonly Queue<InputEventData> _pendingEvents = new();

    public static EventDispatcher<InputEvent> KeyDown = new();
    public static EventDispatcher<InputEvent> KeyUp = new();
    public static EventDispatcher<InputEvent> Press = new();
    private static readonly InputState _currentState = new();

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

    public class InputState
    {
        public readonly HashSet<KeyboardKey> PressedKeys = new();
    }
}