// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Graphics;

public struct ComplexColor
{
    public Color TopLeft;
    public Color BottomLeft;
    public Color TopRight;
    public Color BottomRight;
    public bool HasSingleColor;

    public static readonly ComplexColor BLACK = Single(Color.Black);

    public readonly Color SingleColor
    {
        get
        {
            Debug.Assert(HasSingleColor);
            return TopLeft;
        }
    }

    public static ComplexColor Single(Color color)
    {
        var complexColor = new ComplexColor();
        complexColor.TopLeft = complexColor.BottomLeft = complexColor.TopRight = complexColor.BottomRight = color;
        complexColor.HasSingleColor = true;
        return complexColor;
    }

    public static ComplexColor GradientHorizontal(Color left, Color right)
    {
        ComplexColor complexColor = new ComplexColor();
        complexColor.TopLeft = complexColor.BottomLeft = left;
        complexColor.TopRight = complexColor.BottomRight = right;
        complexColor.HasSingleColor = false;
        return complexColor;
    }

    public static ComplexColor GradientVertical(Color top, Color bottom)
    {
        ComplexColor result = new ComplexColor();
        result.TopLeft = result.TopRight = top;
        result.BottomLeft = result.BottomRight = bottom;
        result.HasSingleColor = false;
        return result;
    }

    public readonly Color Interpolate(Vector2 interpolation) => ColorUtil.FromVector(
        (1 - interpolation.Y) * ((1 - interpolation.X) * TopLeft.ToVector() + interpolation.X * TopRight.ToVector()) +
        interpolation.Y * ((1 - interpolation.X) * BottomLeft.ToVector() + interpolation.X * BottomRight.ToVector()));

    public readonly bool Equals(ComplexColor other)
    {
        if (HasSingleColor) return other.HasSingleColor && SingleColor.IsSameAs(other.SingleColor);

        if (other.HasSingleColor)
            return false;

        return
            TopLeft.IsSameAs(other.TopLeft) &&
            TopRight.IsSameAs(other.TopRight) &&
            BottomLeft.IsSameAs(other.BottomLeft) &&
            BottomRight.IsSameAs(other.BottomRight);

    }
}
