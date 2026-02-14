// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Buffers;

namespace Common.Util;

public static class Snapshot
{
    public static Snapshot<T> Rent<T>(List<T> list)
    {
        int count = 0;

        var arraySnapshot = ArrayPool<T>.Shared.Rent(list.Count);
        foreach (var child in list)
        {
            arraySnapshot[count++] = child;
        }
        return new Snapshot<T>(arraySnapshot, count);
    }
}

public readonly struct Snapshot<T>(T[] array, int count)
{
    public readonly T[] Array = array;
    public readonly int Count = count;

    public void Return()
    {
        ArrayPool<T>.Shared.Return(Array, clearArray: true);
    }
}
