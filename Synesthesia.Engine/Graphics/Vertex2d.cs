// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Vertex2d(
    Vector2 position,
    Vector2 texCoord,
    Vector2 size,
    uint color,
    float alpha,
    float radius,
    Vector2 localUv,
    VertexMode mode,
    float borderThickness,
    bool hasSingleColor,
    Matrix4x4 borderColor
)
{
    [VertexInfo(0, 2, VertexAttribPointerType.Float)]
    public readonly Vector2 Position = position;

    [VertexInfo(1, 2, VertexAttribPointerType.Float)]
    public readonly Vector2 TextureCoord = texCoord;

    [VertexInfo(2, 2, VertexAttribPointerType.Float)]
    public readonly Vector2 Size = size;

    [VertexInfo(3, 4, VertexAttribPointerType.UnsignedByte, normalized: true)]
    public readonly uint Color = color;

    [VertexInfo(4, 1, VertexAttribPointerType.Float)]
    public readonly float Alpha = alpha;

    [VertexInfo(5, 1, VertexAttribPointerType.Float)]
    public readonly float Radius = radius;

    [VertexInfo(6, 2, VertexAttribPointerType.Float)]
    public readonly Vector2 LocalUV = localUv;

    [VertexInfo(7, 1, VertexAttribPointerType.Float)]
    public readonly float Mode = (float)mode;

    [VertexInfo(8, 1, VertexAttribPointerType.Float)]
    public readonly float BorderThickness = borderThickness;

    [VertexInfo(9, 1, VertexAttribPointerType.Float)]
    public readonly float BorderHasSingleColor = hasSingleColor.ToInt();

    // can't fucking pass 16 at once. have to break it into chunks. fucking glsl
    [VertexInfo(10, 4, VertexAttribPointerType.Float)]
    [VertexInfo(11, 4, VertexAttribPointerType.Float)]
    [VertexInfo(12, 4, VertexAttribPointerType.Float)]
    [VertexInfo(13, 4, VertexAttribPointerType.Float)]
    public readonly Matrix4x4 BorderColor = borderColor;
}
