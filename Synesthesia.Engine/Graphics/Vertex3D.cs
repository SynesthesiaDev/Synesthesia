// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace Synesthesia.Engine.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Vertex3D
(
    Vector3 position,
    uint color,
    Vector2 texCoord
) : IVertex
{
    [VertexInfo(0, 3, VertexAttribPointerType.Float)]
    public readonly Vector3 Position = position;

    [VertexInfo(3, 4, VertexAttribPointerType.UnsignedByte, normalized: true)]
    public readonly uint Color = color;

    [VertexInfo(1, 2, VertexAttribPointerType.Float)]
    public readonly Vector2 TextureCoord = texCoord;
}
