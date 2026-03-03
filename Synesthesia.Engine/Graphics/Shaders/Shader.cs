// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Statistics;
using Raylib_cs;

namespace Synesthesia.Engine.Graphics.Shaders;

public class Shader(string shaderCode, ShaderType shaderType)
{
    private NativeShader? nativeShader;

    public NativeShader NativeShader
    {
        get
        {
            EnsureCompiled();
            return nativeShader!.Value;
        }
    }

    public readonly ShaderType Type = shaderType;

    public bool IsCompiled => nativeShader.HasValue;

    public void EnsureCompiled()
    {
        if (IsCompiled) return;
        EngineStatistics.SHADER_BINDS.Increment();
        nativeShader = Type == ShaderType.Fragment ? Raylib.LoadShaderFromMemory(null, shaderCode) : Raylib.LoadShaderFromMemory(shaderCode, null);
    }
}
