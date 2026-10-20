// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Platform.Render;

namespace Synesthesia.Engine.Shaders;

public static class BuiltInShaders
{
    public static Shader DefaultShader2D = null!;
    public static Shader StencilShader2D = null!;
    public static Shader DefaultShader3D = null!;

    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        var gl = graphicsDevice.OpenGL;
        DefaultShader2D = new Shader("Default 2D", ShaderKind.Two, gl, ShaderSources.Default2dVertex, ShaderSources.Default2dFragment);
        StencilShader2D = new Shader("Stencil 2D", ShaderKind.Two, gl, ShaderSources.Default2dVertex, ShaderSources.Stencil2dFragment);

        DefaultShader3D = new Shader("Default 3D", ShaderKind.Three, gl, ShaderSources.Default3dVertex, ShaderSources.Default3dFragment);
    }
}
