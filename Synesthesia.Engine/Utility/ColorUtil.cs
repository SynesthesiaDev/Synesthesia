using System.Globalization;
using Raylib_cs;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Utility;

public static class ColorUtil
{
    private static Dictionary<string, Color> _cachedColors = new();

    public static Color GetOrCacheColor(string hex)
    {
        hex = hex.RemovePrefix("#");
        if (_cachedColors.TryGetValue(hex, out var color)) return color!;

        var newColor = FromHex(hex);
        _cachedColors[hex] = newColor;
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
                red = int.Parse(hexColor[0] + hexColor[0].ToString(), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hexColor[1] + hexColor[1].ToString(), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hexColor[2] + hexColor[2].ToString(), NumberStyles.AllowHexSpecifier);
                break;
        }

        return new Color(red, green, blue, alpha);
    }
}