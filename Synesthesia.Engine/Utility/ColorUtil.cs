using System.Globalization;
using Raylib_cs;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Utility;

public static class ColorUtil
{
    private static readonly Dictionary<string, Color> cached_colors = new();

    public static Color ChangeBrightness(this Color color, float correctionFactor)
    {
        var factor = Math.Clamp(correctionFactor, -1f, 1f);

        var red = (float)color.R;
        var green = (float)color.G;
        var blue = (float)color.B;

        if (factor < 0)
        {
            factor = 1 + factor;
            red *= factor;
            green *= factor;
            blue *= factor;
        }
        else
        {
            red = (255 - red) * factor + red;
            green = (255 - green) * factor + green;
            blue = (255 - blue) * factor + blue;
        }

        return new Color(
            (byte)Math.Clamp(red, 0, 255), 
            (byte)Math.Clamp(green, 0, 255), 
            (byte)Math.Clamp(blue, 0, 255), 
            color.A
        );
    }

    public static Color GetOrCacheColor(string hex)
    {
        hex = hex.RemovePrefix("#");
        if (cached_colors.TryGetValue(hex, out var color)) return color!;

        var newColor = FromHex(hex);
        cached_colors[hex] = newColor;
        return newColor;
    }

    public static Color Empty => Color.Black with { A = 0 };

    public static Color FromHex(string hexColor)
    {
        hexColor = hexColor.RemovePrefix("#");

        var red = 0;
        var green = 0;
        var blue = 0;
        var alpha = 255;

        switch (hexColor.Length)
        {
            //#RRGGBB
            case 6:
                red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
                break;
            //#RGB
            case 3:
                break;
        }

        return new Color(red, green, blue, alpha);
    }
}