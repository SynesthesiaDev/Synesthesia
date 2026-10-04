// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;
using Codon.Binary;

namespace Synesthesia.Engine.Physics;

[StructLayout(LayoutKind.Auto)]
public readonly struct AABB2D(Vector2 min, Vector2 max)
{
    public static readonly AABB2D ZERO = new AABB2D(Vector2.Zero, Vector2.Zero);

    public static readonly IBinaryCodec<AABB2D> BINARY_CODEC = BinaryCodecs.For<AABB2D>()
        .Field(BinaryCodecs.VECTOR_2, c => c.Min)
        .Field(BinaryCodecs.VECTOR_2, c => c.Max)
        .Build((min, max) => new AABB2D(min, max));

    public Vector2 Min { get; } = min;
    public Vector2 Max { get; } = max;

    public bool Intersects(AABB2D other)
    {
        return (Min.X <= other.Max.X && Max.X >= other.Min.X) &&
               (Min.Y <= other.Max.Y && Max.Y >= other.Min.Y);
    }

    public bool Contains(Vector2 point)
    {
        return point.X >= Min.X && point.X <= Max.X &&
               point.Y >= Min.Y && point.Y <= Max.Y;
    }

    public Vector4 ToVector4() => new Vector4(Min.X, Min.Y, Max.X, Max.Y);

    public override readonly int GetHashCode() => HashCode.Combine(Min, Max);
}
