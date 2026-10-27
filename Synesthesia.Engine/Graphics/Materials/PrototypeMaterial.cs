// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Shaders;

namespace Synesthesia.Engine.Graphics.Materials;

public sealed class PrototypeMaterial : Material
{
    private const string tint_uniform_name = "u_tint";

    public override string ResourceName { get; } = "Debug Material";
    public override Shader Shader { get; init; } = BuiltInShaders.DebugShader3D;

    private Vector4 cachedTintVector = Color.White.ToVector();
    private int tintUniformLocation = -1;

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

    protected override string Stringify()
    {
        return $"PrototypeMaterial(Name={ResourceName}, Tint={Tint.ToString()})";
    }

    public override void Bind(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.BindShader(Shader);
        Shader.SetVector4(tintUniformLocation, cachedTintVector);
    }

    public override void CacheUniformLocations()
    {
        tintUniformLocation = Shader.GetUniformLocation(tint_uniform_name);
    }

    protected override int ComputeBatchKey() => HashCode.Combine(base.ComputeBatchKey(), cachedTintVector);


    protected override void OnDispose()
    {
    }
}

