// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Extensions;

public static class EnumerableExtensions
{
    public static string AsString<T>(this IEnumerable<T> array) => $"[{string.Join(", ", array)}";
}
