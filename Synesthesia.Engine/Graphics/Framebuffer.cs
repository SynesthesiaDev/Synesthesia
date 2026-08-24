// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace Synesthesia.Engine.Graphics;

[StructLayout(LayoutKind.Auto)]
public readonly struct Framebuffer(uint fbo, uint colorTexture, Vector2 size, PixelFormat pixelFormat)
{
    public readonly uint Fbo = fbo;
    public readonly uint ColorTexture = colorTexture;
    public readonly Vector2 Size = size;
    public readonly PixelFormat PixelFormat = pixelFormat;
}
