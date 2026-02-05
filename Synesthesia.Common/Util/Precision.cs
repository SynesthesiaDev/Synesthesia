// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Common.Util;

public static class Precision
{
    public static bool Equals(float value1, float value2, float precision = 0f)
    {
        return Math.Abs(value1 - value2) < precision;
    }

    // public static bool DefinitelyBigger(float value1, float value2, float precision = 0f)
    // {
    //
    // }

}
