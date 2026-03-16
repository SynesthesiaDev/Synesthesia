// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Util.Pooling;

public readonly struct Snapshot<T>(T[] array, int count) : IDisposable
{
    public readonly int Count = count;

    public Span<T> Span => array.AsSpan(0, Count);

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(array, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }
}

public static class Snapshot
{
    [SuppressMessage("Design", "MA0016:Prefer using collection abstraction instead of implementation")]
    public static Snapshot<T> Rent<T>(List<T> list)
    {
        int count = list.Count;
        if (count == 0) return new Snapshot<T>([], 0);

        T[] arraySnapshot = ArrayPool<T>.Shared.Rent(count);

        CollectionsMarshal.AsSpan(list).CopyTo(arraySnapshot);

        return new Snapshot<T>(arraySnapshot, count);
    }
}
