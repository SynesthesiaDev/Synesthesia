// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Resources;

namespace Synesthesia.Engine.Shaders;

public static class BuiltInShaders
{
    public static Shader DefaultShader2D = null!;
    public static Shader StencilShader2D = null!;
    public static Shader DefaultShader3D = null!;
    public static Shader PrototypeShader3D = null!;
    public static Shader SkyboxShader3D = null!;

    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        var shaderStore = DependencyContainer.Get<IResourceStore<Shader>>();

        DefaultShader2D = shaderStore.Get("Synesthesia.Resources.Shaders.Two.default.glsl");
        StencilShader2D = shaderStore.Get("Synesthesia.Resources.Shaders.Two.stencil.glsl");
        DefaultShader3D = shaderStore.Get("Synesthesia.Resources.Shaders.Three.default.glsl");
        PrototypeShader3D = shaderStore.Get("Synesthesia.Resources.Shaders.Three.prototype.glsl");
        SkyboxShader3D = shaderStore.Get("Synesthesia.Resources.Shaders.Three.skybox.glsl");

        DefaultShader2D.Upload(graphicsDevice);
        StencilShader2D.Upload(graphicsDevice);
        DefaultShader3D.Upload(graphicsDevice);
        PrototypeShader3D.Upload(graphicsDevice);
        SkyboxShader3D.Upload(graphicsDevice);
    }
}
