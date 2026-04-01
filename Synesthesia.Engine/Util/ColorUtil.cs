// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics;

namespace Synesthesia.Engine.Util;

public static class ColorUtil
{
    public static Color FromComponentState(bool condition, bool hovered, Color trueColor, Color falseColor, float lightenFactor)
    {
        var condColor = condition ? trueColor : falseColor;
        return hovered ? condColor.Lighten(lightenFactor) : condColor;
    }

    public static ComplexColor FromComponentState(bool condition, bool hovered, ComplexColor trueColor, ComplexColor falseColor, float lightenFactor)
    {
        var condColor = condition ? trueColor : falseColor;
        return hovered ? condColor.Lighten(lightenFactor) : condColor;
    }

    public static Color FromComponentState(bool hovered, Color color, float lightenFactor) => hovered ? color.Lighten(lightenFactor) : color;
}
