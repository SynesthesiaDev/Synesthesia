// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.CompilerServices;
using Faster.Map.Core;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Util.Exceptions;

namespace Synesthesia.Engine.Graphics;

public class Shader : IDisposable
{
    public const string TRANSFORM_UNIFORM_NAME = "u_transform";

    private readonly GL gl;
    private readonly uint program;

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

        program = gl.CreateProgram();

        if (vertexShader != null) gl.AttachShader(program, vertexShader.Value);
        if (fragmentShader != null) gl.AttachShader(program, fragmentShader.Value);
        gl.LinkProgram(program);

        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int success);

        if (success == 0) throw new OpenGLException($"Shader linking failed: {gl.GetProgramInfoLog(program)}");

        if (vertexShader != null) gl.DeleteShader(vertexShader.Value);
        if (fragmentShader != null) gl.DeleteShader(fragmentShader.Value);
    }

    public int GetUniformLocation(string uniform)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        if (uniformCache.Get(uniform, out int cached))
            return cached;

        var location = gl.GetUniformLocation(program, uniform);
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
            gl.UniformMatrix4(location, 1, false, (float*)Unsafe.AsPointer(ref matrix));
        }
    }

    public void SetFloat(string name, float value)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var location = GetUniformLocation(name);
        gl.Uniform1(location, value);
    }

    public void SetDouble(string name, double value)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var location = GetUniformLocation(name);
        gl.Uniform1(location, value);
    }

    public void SetInt(string name, int value)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var location = GetUniformLocation(name);
        gl.Uniform1(location, value);
    }

    public void SetVector2(string name, Vector2 value)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var location = GetUniformLocation(name);
        gl.Uniform2(location, value);
    }

    public void SetVector3(string name, Vector3 value)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var location = GetUniformLocation(name);
        gl.Uniform3(location, value);
    }

    public void SetVector4(string name, Vector4 value)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var location = GetUniformLocation(name);
        gl.Uniform4(location, value);
    }

    public void Use()
    {
        ThreadSafety.AssertRunningOnRenderThread();
        gl.UseProgram(program);
    }

    public void Dispose()
    {
        gl.DeleteProgram(program);
    }
}
