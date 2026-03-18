// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Graphics;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vertex(Vector2 position, Vector2 texCoord, Color color)
{
    public readonly Vector2 Position = position;
    public readonly Vector2 TextureCoord = texCoord;
    public readonly Color Color = color;
}
