// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Utility;

public static class MathUtil
{
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
