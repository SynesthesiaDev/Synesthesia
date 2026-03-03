// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Graphics.Renderer;

public interface IRenderer
{
    string Name { get; }

    void Enable(int flag);

    void Disable(int flag);

    void Clear(int mask);

    void ClearStencil(int stencil);

    void StencilFunc(int func, int @ref, uint mask);

    void StencilOp(int fail, int zfail, int zpass);

    void ColorMask(bool red, bool green, bool blue, bool alpha);

    void GetIntegerv(int pname, out int data);
}
