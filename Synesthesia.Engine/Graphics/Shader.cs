// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Faster.Map.Core;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Util.Exceptions;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Graphics;

public class Shader : IDisposable
{
    public const string TRANSFORM_UNIFORM_NAME = "u_transform";

    private readonly GL gl;
    public readonly uint Program;

    private readonly DenseMap<string, int> uniformCache = new();

    public Shader(GL openGl, string? vertexCode, string? fragmentCode)
    {
        ThreadSafety.AssertRunningOnRenderThread();

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
        return location != -1 ? location : throw new OpenGLException($"Failed to get shader uniform '{uniform}'");
    }

    private uint compileShader(ShaderType shaderType, string code)
    {
        uint shader = gl.CreateShader(shaderType);
        gl.ShaderSource(shader, code);
        gl.CompileShader(shader);

        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);

        var shaderId = success == 0 ? throw new OpenGLException($"Shader compilation failed: ({shaderType}): {gl.GetShaderInfoLog(shader)}") : shader;
        Logger.Verbose($"Compiled {shaderType.ToString().Replace("Shader", string.Empty)} shader with id {shaderId}", Logger.Render);
        return shaderId;
    }

    public void SetMatrix4(string name, Matrix4x4 matrix)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        int location = GetUniformLocation(name);
        unsafe
        {
            gl.UniformMatrix4(location, 1, false, (float*)&matrix);
        }
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetFloat(string name, float value)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var location = GetUniformLocation(name);
        gl.Uniform1(location, value);
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetDouble(string name, double value)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var location = GetUniformLocation(name);
        gl.Uniform1(location, value);
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetInt(string name, int value)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var location = GetUniformLocation(name);
        gl.Uniform1(location, value);
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetVector2(string name, Vector2 value)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var location = GetUniformLocation(name);
        gl.Uniform2(location, value);
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetVector3(string name, Vector3 value)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var location = GetUniformLocation(name);
        gl.Uniform3(location, value);
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
    }

    public void SetVector4(string name, Vector4 value)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var location = GetUniformLocation(name);
        gl.Uniform4(location, value);
        DrawStatistics.Increment(DrawStatistics.Type.UniformUploads);
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

    public override string ToString()
    {
        return $"Shader(program={Program},cacheSize={uniformCache.Size})";
    }
}
