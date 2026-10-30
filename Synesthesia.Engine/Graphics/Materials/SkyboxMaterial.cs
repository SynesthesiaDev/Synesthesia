// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Shaders;
using Texture = Synesthesia.Engine.Graphics.Textures.Texture;

namespace Synesthesia.Engine.Graphics.Materials;

public sealed class SkyboxMaterial(string name) : Material
{
    private const string tint_uniform_name = "u_tint";

    public override string ResourceName => name;
    public override Shader Shader { get; init; } = BuiltInShaders.SkyboxShader3D;

    public override TextureFilterMode TextureFilterMode => TextureFilterMode.Linear;

    public Texture? Texture { get; init; }

    public Color Tint
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            cachedTintVector = value.ToVector();
        }
    } = Color.White;

    private Vector4 cachedTintVector = Color.White.ToVector();
    private int tintUniformLocation = -1;

    public override void Bind(GraphicsDevice graphicsDevice)
    {
        // 1. Disable culling so we can see the inside of the cube
        graphicsDevice.OpenGL.Disable(EnableCap.CullFace);

        // 2. Allow drawing at max depth (1.0) and don't write to the depth buffer
        graphicsDevice.OpenGL.DepthFunc(DepthFunction.Lequal);
        graphicsDevice.OpenGL.DepthMask(false);

        graphicsDevice.BindShader(Shader);
        if (Texture != null) graphicsDevice.BindTexture(Texture, TextureFilterMode);
        Shader.SetVector4(tintUniformLocation, cachedTintVector);
    }

    public override void CacheUniformLocations()
    {
        tintUniformLocation = Shader.GetUniformLocation(tint_uniform_name);
    }

    protected override int ComputeBatchKey() => HashCode.Combine(base.ComputeBatchKey(), Texture?.Handle ?? 0, cachedTintVector);

    public static SkyboxMaterial From(Texture texture) => new SkyboxMaterial(texture.ResourceName) { Texture = texture };
    public static SkyboxMaterial From(Texture texture, Color tint) => new SkyboxMaterial(texture.ResourceName) { Texture = texture, Tint = tint };

    public static SkyboxMaterial From(string name, Texture texture) => new SkyboxMaterial(name) { Texture = texture };
    public static SkyboxMaterial From(string name, Texture texture, Color tint) => new SkyboxMaterial(name) { Texture = texture, Tint = tint };

    protected override void OnDispose()
    {
    }

    protected override string Stringify() => $"SkyboxMaterial(Name={ResourceName}, Texture={Texture?.ResourceName ?? "null"}, Tint={Tint.ToString()})";
}
