// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.CompilerServices;
using Faster.Map.Core;

namespace Synesthesia.Engine.Extensions;

public static class MapExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TV? GetOrNullStruct<TK, TV>(this IDictionary<TK, TV> map, TK key) where TV : struct
    {
        if (map.TryGetValue(key, out var keyValuePair)) return keyValuePair;
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TV? GetOrNull<TK, TV>(this IDictionary<TK, TV> map, TK key) where TV : class
    {
        return map.TryGetValue(key, out var keyValuePair) ? keyValuePair : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TV? GetOrNullStruct<TK, TV>(this DenseMap<TK, TV> map, TK key) where TV : struct
    {
        if (map.Get(key, out var value)) return value;
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TV? GetOrNullClass<TK, TV>(this DenseMap<TK, TV> map, TK key) where TV : class
    {
        return map.Get(key, out var value) ? value : null;
    }

}
