// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace Synesthesia.Engine.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Vertex2d(Vector2 position, Vector2 texCoord, uint color)
{
    [VertexInfo(0, 2, VertexAttribPointerType.Float)]
    public readonly Vector2 Position = position;

    [VertexInfo(1, 2, VertexAttribPointerType.Float)]
    public readonly Vector2 TextureCoord = texCoord;

    [VertexInfo(2, 4, VertexAttribPointerType.UnsignedByte, true)]
    public readonly uint Color = color;
}
