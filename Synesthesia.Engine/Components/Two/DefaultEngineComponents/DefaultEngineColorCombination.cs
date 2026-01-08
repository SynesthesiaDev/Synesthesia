using Raylib_cs;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public record DefaultEngineColorCombination(Color Normal, Color Hovered)
{
    public static readonly DefaultEngineColorCombination Surface0 = new(Defaults.Background0, Defaults.Background1);
    public static readonly DefaultEngineColorCombination Surface1 = new(Defaults.Background1, Defaults.Background2);
    public static readonly DefaultEngineColorCombination Surface2 = new(Defaults.Background2, Defaults.Background3);
    public static readonly DefaultEngineColorCombination Surface3 = new(Defaults.Background3, Defaults.Background4);
    public static readonly DefaultEngineColorCombination Surface4 = new(Defaults.Background4, Defaults.Background5);
    public static readonly DefaultEngineColorCombination Accent = new(Defaults.Accent, Defaults.Accent.ChangeBrightness(0.4f));
    
}