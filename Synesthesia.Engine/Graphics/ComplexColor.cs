// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct ComplexColor : IEquatable<ComplexColor>
{
    public Color TopLeft;
    public Color BottomLeft;
    public Color TopRight;
    public Color BottomRight;
    public bool HasSingleColor;

    public readonly Color SingleColor
    {
        get
        {
            Debug.Assert(HasSingleColor);
            return TopLeft;
        }
    }

    public readonly Matrix4x4 ToMatrix4()
    {
        return new Matrix4x4(
            TopLeft.R, TopLeft.G, TopLeft.B, TopLeft.A,
            TopRight.R, TopRight.G, TopRight.B, TopRight.A,
            BottomLeft.R, BottomLeft.G, BottomLeft.B, BottomLeft.A,
            BottomRight.R, BottomRight.G, BottomRight.B, BottomRight.A
        );
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
        var complexColor = new ComplexColor();
        complexColor.TopLeft = complexColor.BottomLeft = left;
        complexColor.TopRight = complexColor.BottomRight = right;
        complexColor.HasSingleColor = false;
        return complexColor;
    }

    public static ComplexColor GradientVertical(Color top, Color bottom)
    {
        var result = new ComplexColor();
        result.TopLeft = result.TopRight = top;
        result.BottomLeft = result.BottomRight = bottom;
        result.HasSingleColor = false;
        return result;
    }

    public ComplexColor Multiply(float scalar)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(scalar);
        if (HasSingleColor) return ComplexColor.Single(TopLeft.Multiply(scalar));

        return new ComplexColor()
        {
            TopLeft = TopLeft.Multiply(scalar),
            TopRight = TopRight.Multiply(scalar),
            BottomLeft = BottomLeft.Multiply(scalar),
            BottomRight = BottomRight.Multiply(scalar),
        };
    }

    /// <summary>
    /// Returns a lightened version of the color.
    /// </summary>
    /// <param name="amount">Decimal light addition</param>
    public ComplexColor Lighten(float amount) => Multiply(1 + amount);

    /// <summary>
    /// Returns a darkened version of the color.
    /// </summary>
    /// <param name="amount">Percentage light reduction</param>
    public ComplexColor Darken(float amount) => Multiply(1 / (1 + amount));

    public static ComplexColor Custom(Color topLeft, Color topRight, Color bottomLeft, Color bottomRight)
    {
        var result = new ComplexColor
        {
            TopLeft = topLeft,
            TopRight = topRight,
            BottomLeft = bottomLeft,
            BottomRight = bottomRight,
            HasSingleColor = false,
        };

        return result;
    }

    public readonly Color Interpolate(Vector2 interpolation)
    {
        if (HasSingleColor) return TopLeft;

        var top = Vector4.Lerp(TopLeft.ToVector(), TopRight.ToVector(), interpolation.X);
        var bottom = Vector4.Lerp(BottomLeft.ToVector(), BottomRight.ToVector(), interpolation.X);
        return Color.FromVector(Vector4.Lerp(top, bottom, interpolation.Y));
    }

    public static bool operator ==(ComplexColor left, ComplexColor right) => left.Equals(right);
    public static bool operator !=(ComplexColor left, ComplexColor right) => !left.Equals(right);

    public readonly bool Equals(ComplexColor other)
    {
        if (HasSingleColor != other.HasSingleColor) return false;

        if (HasSingleColor)
            return TopLeft == other.TopLeft;

        return TopLeft == other.TopLeft &&
               BottomLeft == other.BottomLeft &&
               TopRight == other.TopRight &&
               BottomRight == other.BottomRight;
    }


    public override readonly bool Equals(object? obj) => obj is ComplexColor color && Equals(color);

    public override readonly int GetHashCode()
    {
        if (HasSingleColor) return TopLeft.ToArgb() ^ 0x12345678;

        unchecked
        {
            int hash = TopLeft.ToArgb();
            hash = (hash * 397) ^ BottomLeft.ToArgb();
            hash = (hash * 397) ^ TopRight.ToArgb();
            hash = (hash * 397) ^ BottomRight.ToArgb();
            return hash;
        }
    }

    public static readonly ComplexColor Black = Single(Color.Black);
    public static readonly ComplexColor White = Single(Color.White);
    public static readonly ComplexColor Transparent = Single(Color.Transparent);
}
