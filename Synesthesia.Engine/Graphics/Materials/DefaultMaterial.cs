// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Shaders;

namespace Synesthesia.Engine.Graphics.Materials;

public sealed class DefaultMaterial(string name) : Material
{
    private const string tint_uniform_name = "u_tint";

    public override string ResourceName => name;
    public override Shader Shader { get; init; } = BuiltInShaders.DefaultShader3D;
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
        graphicsDevice.BindShader(Shader);
        if (Texture != null) graphicsDevice.BindTexture(Texture, TextureFilterMode);
        Shader.SetVector4(tintUniformLocation, cachedTintVector);
    }

    public override void CacheUniformLocations()
    {
        tintUniformLocation = Shader.GetUniformLocation(tint_uniform_name);
    }

    protected override int ComputeBatchKey() => HashCode.Combine(base.ComputeBatchKey(), Texture?.Handle ?? 0, cachedTintVector);

    public static DefaultMaterial From(Texture texture) => new DefaultMaterial(texture.ResourceName) { Texture = texture };
    public static DefaultMaterial From(Texture texture, Color tint) => new DefaultMaterial(texture.ResourceName) { Texture = texture, Tint = tint };
    public static DefaultMaterial From(Color tint) => new DefaultMaterial($"_color{tint.ToStringRaw()}") { Tint = tint };

    public static DefaultMaterial From(string name, Texture texture) => new DefaultMaterial(name) { Texture = texture };
    public static DefaultMaterial From(string name, Texture texture, Color tint) => new DefaultMaterial(name) { Texture = texture, Tint = tint };
    public static DefaultMaterial From(string name, Color tint) => new DefaultMaterial(name) { Tint = tint };

    protected override void OnDispose()
    {
    }

    protected override string Stringify() => $"SimpleMaterial(Name={ResourceName}, Texture={Texture?.ResourceName ?? "null"}, Tint={Tint.ToString()})";
}
