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
    Vector3 normal,
    Vector3 tangent,
    uint color,
    Vector2 texCoord
) : IVertex
{
    [VertexInfo(0, 3, VertexAttribPointerType.Float)]
    public readonly Vector3 Position = position;

    [VertexInfo(1, 3, VertexAttribPointerType.Float)]
    public readonly Vector3 Normal = normal;

    [VertexInfo(2, 3, VertexAttribPointerType.Float)]
    public readonly Vector3 Tangent = tangent;

    [VertexInfo(3, 4, VertexAttribPointerType.UnsignedByte, normalized: true)]
    public readonly uint Color = color;

    [VertexInfo(4, 2, VertexAttribPointerType.Float)]
    public readonly Vector2 TextureCoord = texCoord;
}

