using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Utility;
using SynesthesiaUtil;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Input;

public class ActionBinding
{
    public string ActionName { get; set; } = string.Empty;
    public KeyboardBinding? Key { get; set; }
    public GamepadBinding? PadButton { get; set; }
    public TouchBinding? TouchGesture { get; set; }
    public MouseBinding? MouseButton { get; set; }

    private List<IBinding?> AsBindingList()
    {
        return Lists.Of<IBinding?>(Key, PadButton, TouchGesture, MouseButton);
    }

    public bool IsDown => AsBindingList().FilterNonNull().Any(b => b!.IsDown());
}

public interface IBinding
{
    public bool IsDown();
}

public record KeyboardBinding(KeyboardKey Key, params KeyboardKey[] Modifiers) : IBinding
{
    public bool IsDown() => Key.IsDown() && Modifiers.All(key => key.IsDown());
}

public record GamepadBinding(GamepadButton Key, params GamepadButton[] Modifiers) : IBinding
{
    public bool IsDown() => false;
}

public record TouchBinding(Gesture Key, params Gesture[] Modifiers) : IBinding
{
    public bool IsDown() => false;
}

public record MouseBinding(MouseButton Key, params MouseButton[] Modifiers) : IBinding
{
    public bool IsDown() => Key.IsDown() && Modifiers.All(button => button.IsDown());
}