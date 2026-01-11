using Raylib_cs;

namespace Synesthesia.Engine.Input;

public class ActionBinding
{
    public string ActionName { get; set; } = string.Empty;
    public KeyboardBinding? Key { get; set; }
    public GamepadBinding? PadButton { get; set; }
    public TouchBinding? TouchGesture { get; set; }
    public MouseBinding? MouseButton { get; set; }
}

public record KeyboardBinding(KeyboardKey Key, params KeyboardKey[] Modifiers);
public record GamepadBinding(GamepadButton Key, params GamepadButton[] Modifiers);
public record TouchBinding(Gesture Key, params Gesture[] Modifiers);
public record MouseBinding(MouseButton Key, params MouseButton[] Modifiers);