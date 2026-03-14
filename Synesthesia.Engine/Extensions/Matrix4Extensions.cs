// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;

namespace Synesthesia.Engine.Extensions;

public static class Matrix4Extensions
{

    extension(Matrix4x4 matrix)
    {
        public Vector4 GetColumn(int column)
        {
            return column switch
            {
                0 => new Vector4(matrix.M11, matrix.M21, matrix.M31, matrix.M41),
                1 => new Vector4(matrix.M12, matrix.M22, matrix.M32, matrix.M42),
                2 => new Vector4(matrix.M13, matrix.M23, matrix.M33, matrix.M43),
                3 => new Vector4(matrix.M14, matrix.M24, matrix.M34, matrix.M44),
                _ => throw new ArgumentOutOfRangeException(nameof(column)),
            };
        }

        public Matrix4x4 WithColumn(int column, Vector4 value)
        {
            return column switch
            {
                0 => matrix with { M11 = value.X, M21 = value.Y, M31 = value.Z, M41 = value.W },
                1 => matrix with { M12 = value.X, M22 = value.Y, M32 = value.Z, M42 = value.W },
                2 => matrix with { M13 = value.X, M23 = value.Y, M33 = value.Z, M43 = value.W },
                3 => matrix with { M14 = value.X, M24 = value.Y, M34 = value.Z, M44 = value.W },
                _ => throw new ArgumentOutOfRangeException(nameof(column)),
            };
        }
    }

    /// <summary>
    /// Rotates the matrix around the Z-axis by left-multiplying
    /// </summary>
    public static void RotateFromLeft(ref Matrix4x4 matrix, float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);

        var row0 = matrix.GetRow(0);
        var row1 = matrix.GetRow(1);

        matrix = matrix.WithRow(0, row0 * cos + row1 * sin);
        matrix = matrix.WithRow(1, row1 * cos - row0 * sin);
    }

    /// <summary>
    /// Rotates the matrix around the Z-axis by right-multiplying
    /// </summary>
    public static void RotateFromRight(ref Matrix4x4 matrix, float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);

        var col0 = matrix.GetColumn(0);
        var col1 = matrix.GetColumn(1);

        matrix = matrix.WithColumn(0, col0 * cos - col1 * sin);
        matrix = matrix.WithColumn(1, col0 * sin + col1 * cos);
    }
}
