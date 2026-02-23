// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;

namespace Synesthesia.Engine.Utility;

public static class MathUtil
{
    public static double Lerp(double start, double final, double amount) => start + (final - start) * amount;

    public static double Damp(double start, double final, double @base, double exponent)
    {
        if (@base < 0 || @base > 1)
            throw new ArgumentOutOfRangeException(nameof(@base), $"{nameof(@base)} has to lie in [0,1], but is {@base}.");

        return Lerp(start, final, 1 - Math.Pow(@base, exponent));
    }

    public static double DampContinuously(double current, double target, double halfTime, double elapsedTime)
    {
        double exponent = elapsedTime / halfTime;
        return Damp(current, target, 0.5, exponent);
    }

    public static double Lagrange(ReadOnlySpan<Vector2> points, double time)
    {
        if (points == null || points.Length == 0)
            throw new ArgumentException($"{nameof(points)} must contain at least one point");

        double sum = 0;
        for (int i = 0; i < points.Length; i++)
            sum += points[i].Y * LagrangeBasis(points, i, time);
        return sum;
    }

    public static double LagrangeBasis(ReadOnlySpan<Vector2> points, int @base, double time)
    {
        double product = 1;

        for (int i = 0; i < points.Length; i++)
        {
            if (i != @base)
                product *= (time - points[i].X) / (points[@base].X - points[i].X);
        }

        return product;
    }

    public static float ValueOf(float normalizedPercentage, float max)
    {
        float clamped = Math.Clamp(normalizedPercentage, 0f, 1f);
        return clamped * max;
    }

    internal static double LevelToDb(double level)
    {
        if (level <= 0) return -90;

        return 20 * Math.Log10(level);
    }
}
