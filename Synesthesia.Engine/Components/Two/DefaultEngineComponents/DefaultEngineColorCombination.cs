using Raylib_cs;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public record DefaultEngineColorCombination(Color Normal, Color Hovered)
{
    public static readonly DefaultEngineColorCombination SURFACE0 = new(Defaults.BACKGROUND0, Defaults.BACKGROUND1);
    public static readonly DefaultEngineColorCombination SURFACE1 = new(Defaults.BACKGROUND1, Defaults.BACKGROUND2);
    public static readonly DefaultEngineColorCombination SURFACE2 = new(Defaults.BACKGROUND2, Defaults.BACKGROUND3);
    public static readonly DefaultEngineColorCombination SURFACE3 = new(Defaults.BACKGROUND3, Defaults.BACKGROUND4);
    public static readonly DefaultEngineColorCombination SURFACE4 = new(Defaults.BACKGROUND4, Defaults.BACKGROUND5);
    public static readonly DefaultEngineColorCombination ACCENT = new(Defaults.ACCENT, Defaults.ACCENT.ChangeBrightness(0.4f));
    public static readonly DefaultEngineColorCombination RED = new(Defaults.RED, Defaults.RED.ChangeBrightness(0.4f));

}
