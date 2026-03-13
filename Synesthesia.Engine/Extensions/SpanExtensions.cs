// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Extensions;

public static class SpanExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IndexedEnumerator<T> WithIndex<T>(this Span<T> span) => new(span);

    [StructLayout(LayoutKind.Auto)]
    public ref struct IndexedEnumerator<T>(Span<T> span)
    {
        private readonly Span<T> span = span;
        private int index = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++index < span.Length;

        public (int Index, T Value) Current => (index, span[index]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IndexedEnumerator<T> GetEnumerator() => this;
    }
}
