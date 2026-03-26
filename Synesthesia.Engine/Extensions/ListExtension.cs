// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.CompilerServices;

namespace Synesthesia.Engine.Extensions;

public static class ListExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AddIfNotPresent<T>(this IList<T> list, T item)
    {
        if (list.Contains(item)) return false;
        list.Add(item);
        return true;
    }

    extension<T>(IEnumerable<T> list) where T : struct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetOrNullStruct(T item)
        {
            if (list.Contains(item)) return item;
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetOrNullStruct(int index)
        {
            var enumerable = list as T[] ?? list.ToArray();
            if (enumerable.Length - 1 >= index)
            {
                return enumerable[index];
            }
            return null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetOrNullClass<T>(this IEnumerable<T> list, T item) where T : class
    {
        if (list.Contains(item)) return item;
        return null;
    }


}
