// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Vertex2d(Vector2 position, Vector2 texCoord, Color color) : IEquatable<Vertex2d>
{
    public readonly Vector2 Position = position;
    public readonly Vector2 TextureCoord = texCoord;
    public readonly Color Color = color;

    public readonly bool Equals(Vertex2d other) => Position.Equals(other.Position) && Color.Equals(other.Color);
}
