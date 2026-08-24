// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.CompilerServices;
using Synesthesia.Utils.Extensions;

namespace Synesthesia.Engine.Extensions;

public static class EnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasEitherFlag<T>(this T enumValue, params T[] flags) where T : unmanaged, Enum
    {
        foreach (var flag in flags)
        {
            if (enumValue.HasFlagFast(flag)) return true;
        }

        return false;
    }
}
