// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Graphics.Renderer;

public class OpenGlRenderer : IRenderer
{
    public const int GL_STENCIL_TEST = 0x0B90;
    public const int GL_STENCIL_BUFFER_BIT = 0x00000400;
    public const int GL_ALWAYS = 0x0207;
    public const int GL_EQUAL = 0x0202;
    public const int GL_KEEP = 0x1E00;
    public const int GL_REPLACE = 0x1E01;
    public const int GL_STENCIL_BITS = 0x0D57;

    public const int GLFW_STENCIL_BITS = 0x00021006;

    public string Name => "OpenGL";

    public void Enable(int flag) => glEnable(flag);

    public void Disable(int flag) => glDisable(flag);

    public void Clear(int mask) => glClear(mask);

    public void ClearStencil(int stencil) => glClearStencil(stencil);

    public void StencilFunc(int func, int @ref, uint mask) => glStencilFunc(func, @ref, mask);

    public void StencilOp(int fail, int zfail, int zpass) => glStencilOp(fail, zfail, zpass);

    public void ColorMask(bool red, bool green, bool blue, bool alpha) => glColorMask(red, green, blue, alpha);

    public void GetIntegerv(int pname, out int data) => glGetIntegerv(pname, out data);

    [DllImport("opengl32.dll")]
    private static extern void glGetIntegerv(int pname, out int data);

    [DllImport("opengl32.dll")]
    private static extern void glEnable(int cap);

    [DllImport("opengl32.dll")]
    private static extern void glDisable(int cap);

    [DllImport("opengl32.dll")]
    private static extern void glClear(int mask);

    [DllImport("opengl32.dll")]
    private static extern void glClearStencil(int s);

    [DllImport("opengl32.dll")]
    private static extern void glStencilFunc(int func, int @ref, uint mask);

    [DllImport("opengl32.dll")]
    private static extern void glStencilOp(int fail, int zfail, int zpass);


    [DllImport("opengl32.dll")]
    private static extern void glColorMask(bool red, bool green, bool blue, bool alpha);

}
