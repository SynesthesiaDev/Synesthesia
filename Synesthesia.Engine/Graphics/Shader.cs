// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.CompilerServices;
using Faster.Map.Core;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Util.Exceptions;
using Synesthesia.Engine.Util.Statistics;


namespace Synesthesia.Engine.Graphics;

public class Shader : IDisposable, INamedResource
{
    private readonly GL gl;
    public readonly uint Program;
    public readonly ShaderKind Kind;

    private readonly DenseMap<string, int> uniformCache = new();

    public string ResourceName { get; }

    public Shader(string name, ShaderKind kind, GL openGl, string? vertexCode, string? fragmentCode)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        ResourceName = name;
        Kind = kind;
        gl = openGl;

        if (vertexCode == null && fragmentCode == null) throw new OpenGLException("Cannot have both Vertex and Fragment shaders empty");

        uint? vertexShader = null;
        uint? fragmentShader = null;

        if (vertexCode != null) vertexShader = compileShader(ShaderType.VertexShader, vertexCode);
        if (fragmentCode != null) fragmentShader = compileShader(ShaderType.FragmentShader, fragmentCode);

        Program = gl.CreateProgram();

        if (vertexShader != null) gl.AttachShader(Program, vertexShader.Value);
        if (fragmentShader != null) gl.AttachShader(Program, fragmentShader.Value);
        gl.LinkProgram(Program);

        gl.GetProgram(Program, ProgramPropertyARB.LinkStatus, out int success);

        if (success == 0) throw new OpenGLException($"Shader linking failed: {gl.GetProgramInfoLog(Program)}");

        if (vertexShader != null) gl.DeleteShader(vertexShader.Value);
        if (fragmentShader != null) gl.DeleteShader(fragmentShader.Value);
    }

    public int GetUniformLocation(string uniform)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        if (uniformCache.Get(uniform, out int cached))
            return cached;

        var location = gl.GetUniformLocation(Program, uniform);

        if (location != -1)
            uniformCache.Insert(uniform, location);

        return location != -1 ? location : throw new OpenGLException($"Failed to get shader uniform '{uniform}'");
    }

    private uint compileShader(ShaderType shaderType, string code)
    {
        uint shader = gl.CreateShader(shaderType);
        gl.ShaderSource(shader, code);
        gl.CompileShader(shader);

        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);

        var shaderId = success == 0 ? throw new OpenGLException($"Shader compilation failed: ({shaderType}): {gl.GetShaderInfoLog(shader)}") : shader;
        Logger.Verbose($"Compiled {shaderType.ToString().Replace("Shader", string.Empty)} shader '{ResourceName}' with id {shaderId}", Logger.Render);
        return shaderId;
    }

    public void SetMatrix4(int location, Matrix4x4 matrix)
    {
        assertLocationNotInvalid(location);
        ThreadSafety.AssertRunningOnRenderThread();
        unsafe
        {
            gl.UniformMatrix4(location, 1, transpose: false, (float*)&matrix);
        }
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetMatrix4(string name, Matrix4x4 matrix)
    {
        int location = GetUniformLocation(name);
        SetMatrix4(location, matrix);
    }

    public void SetFloat(int location, float value)
    {
        assertLocationNotInvalid(location);
        ThreadSafety.AssertRunningOnRenderThread();
        gl.Uniform1(location, value);
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetFloat(string name, float value)
    {
        var location = GetUniformLocation(name);
        SetFloat(location, value);
    }

    public void SetDouble(int location, double value)
    {
        assertLocationNotInvalid(location);
        ThreadSafety.AssertRunningOnRenderThread();
        gl.Uniform1(location, value);
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetDouble(string name, double value)
    {
        var location = GetUniformLocation(name);
        SetDouble(location, value);
    }

    public void SetInt(int location, int value)
    {
        assertLocationNotInvalid(location);
        ThreadSafety.AssertRunningOnRenderThread();
        gl.Uniform1(location, value);
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetInt(string name, int value)
    {
        var location = GetUniformLocation(name);
        SetInt(location, value);
    }

    public void SetVector2(int location, Vector2 value)
    {
        assertLocationNotInvalid(location);
        ThreadSafety.AssertRunningOnRenderThread();

        gl.Uniform2(location, value);
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetVector2(string name, Vector2 value)
    {
        var location = GetUniformLocation(name);
        SetVector2(location, value);
    }

    public void SetVector3(int location, Vector3 value)
    {
        assertLocationNotInvalid(location);
        ThreadSafety.AssertRunningOnRenderThread();

        gl.Uniform3(location, value);
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetVector3(string name, Vector3 value)
    {
        var location = GetUniformLocation(name);
        SetVector3(location, value);
    }

    public void SetVector4(int location, Vector4 value)
    {
        assertLocationNotInvalid(location);
        ThreadSafety.AssertRunningOnRenderThread();

        gl.Uniform4(location, value);
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetColor(int location, Color value)
    {
        assertLocationNotInvalid(location);
        ThreadSafety.AssertRunningOnRenderThread();

        gl.Uniform4(location, value.ToVector());
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }


    public void SetVector4(string name, Vector4 value)
    {
        var location = GetUniformLocation(name);
        SetVector4(location, value);
    }

    public void Use()
    {
        ThreadSafety.AssertRunningOnRenderThread();
        gl.UseProgram(Program);
        DrawStatistics.Increment(DrawStatistics.Type.ShaderBinds);
    }

    public void Dispose()
    {
        gl.DeleteProgram(Program);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void assertLocationNotInvalid(int location)
    {
        if(location != -1) return;
        throw new OpenGLException("Uninitialized or Invalid shader location -1");
    }

    public override string ToString() => $"Shader(Name={ResourceName}, Program={Program}, CacheSize={uniformCache.Size})";
}
