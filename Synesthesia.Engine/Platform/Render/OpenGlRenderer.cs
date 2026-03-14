// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Util;
using SynesthesiaUtil.Extensions;
using Shader = Synesthesia.Engine.Graphics.Shader;

namespace Synesthesia.Engine.Platform.Render;

public sealed class OpenGlRenderer : IDisposable
{
    private const ClearFlags default_clear_flags = ClearFlags.ColorBuffer | ClearFlags.DepthBuffer | ClearFlags.StencilBuffer;

    public required OpenGLSurface Surface { get; init; }

    private bool openGlInitialized;

    public int BackBufferWidth { get; private set; }

    public int BackBufferHeight { get; private set; }

    public ClearFlags ClearFlags = default_clear_flags;

    private Shader defaultShader = null!;

    public GL OpenGL
    {
        get
        {
            EnsureInitialized();
            return field;
        }

        private set;
    } = null!;

    public bool CanDraw => BackBufferWidth > 0 && BackBufferHeight > 0;

    private readonly Stack<Matrix4x4> matrixStack = new Stack<Matrix4x4>();

    private readonly Stack<Matrix4x4> inverseMatrixStack = new();

    public Shader CurrentShader { get; private set; } = null!;

    public Matrix4x4 Matrix { get; private set; } = Matrix4x4.Identity;

    public Matrix4x4 InverseMatrix { get; private set; } = Matrix4x4.Identity;

    public void Initialize()
    {
        if (openGlInitialized) throw new InvalidOperationException("OpenGL is already initialized");

        var gl = GL.GetApi(name =>
        {
            var ptr = OpenGLSurface.GetProcAddress(name);
            return ptr;
        });

        OpenGL = gl ?? throw new InvalidOperationException("Silk.NET could not bind to OpenGL");

        BackBufferHeight = Surface.BackBufferHeight;
        BackBufferWidth = Surface.BackBufferWidth;

        defaultShader = new Shader(OpenGL, Defaults.DEFAULT_VERTEX_SHADER, Defaults.DEFAULT_FRAGMENT_SHADER);
        BeginShader(defaultShader);

        openGlInitialized = true;
        var version = OpenGL.GetStringS(GLEnum.Version);
        var shadingLanguageVersion = OpenGL.GetStringS(GLEnum.ShadingLanguageVersion);
        var vendor = OpenGL.GetStringS(GLEnum.Vendor);
        var renderer = OpenGL.GetStringS(GLEnum.Renderer);

        Console.WriteLine(string.Empty);
        Logger.Debug("OpenGL Initialized", Logger.Platform);
        Logger.Debug($"- Version:   {version}", Logger.Platform);
        Logger.Debug($"- Vendor:    {vendor}", Logger.Platform);
        Logger.Debug($"- Renderer   {renderer}", Logger.Platform);
        Logger.Debug($"- GLSL:      {shadingLanguageVersion}", Logger.Platform);
    }

    public void BeginShader(Shader shader)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        if (CurrentShader == shader) return;

        CurrentShader = shader;
        shader.Use();
        updateShaderMatrix();
    }

    public void EndShader()
    {
        ThreadSafety.AssertRunningOnRenderThread();
        CurrentShader = defaultShader;
    }

    public void Resize(int width, int height)
    {
        EnsureInitialized();

        BackBufferWidth = width;
        BackBufferHeight = height;
        OpenGL.Viewport(0, 0, (uint)width, (uint)height);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureInitialized()
    {
        if (!openGlInitialized) throw new InvalidOperationException("OpenGL is not initialized yet");
    }

    public void BeginDrawing()
    {
        EnsureInitialized();
        ClearBufferMask mask = ClearBufferMask.None;

        if (ClearFlags.HasFlagFast(ClearFlags.ColorBuffer))
            mask |= ClearBufferMask.ColorBufferBit;
        if (ClearFlags.HasFlagFast(ClearFlags.DepthBuffer))
            mask |= ClearBufferMask.DepthBufferBit;
        if (ClearFlags.HasFlagFast(ClearFlags.StencilBuffer))
            mask |= ClearBufferMask.StencilBufferBit;

        if (mask != ClearBufferMask.None) OpenGL.Clear(mask);
    }

    public void EndDrawing()
    {
        EnsureInitialized();

        Surface.SwapBuffers();

        ClearFlags = default_clear_flags;
    }

    public void PushMatrix()
    {
        EnsureInitialized();
        matrixStack.Push(Matrix);
        inverseMatrixStack.Push(InverseMatrix);
    }

    public void PopMatrix()
    {
        EnsureInitialized();

        if (matrixStack.Count == 0) throw new InvalidOperationException("Matrix stack is empty");

        Matrix = matrixStack.Pop();
        InverseMatrix = inverseMatrixStack.Pop();

        updateShaderMatrix();
    }

    public void Translate(float x, float y, float z)
    {
        EnsureInitialized();

        Matrix = Matrix4x4.CreateTranslation(x, y, z) * Matrix;
        InverseMatrix *= Matrix4x4.CreateTranslation(-x, -y, -z);

        updateShaderMatrix();
    }

    public void Scale(float x, float y, float z)
    {
        EnsureInitialized();
        Matrix = Matrix4x4.CreateScale(x, y, z) * Matrix;
        InverseMatrix *= Matrix4x4.CreateScale(1 / x, 1 / y, 1 / z);

        updateShaderMatrix();
    }

    public void Rotate(float degrees, float x, float y, float z)
    {
        EnsureInitialized();

        var rads = degrees.ToRads();
        var axis = Vector3.Normalize(new Vector3(x, y, z));

        Matrix = Matrix4x4.CreateFromAxisAngle(axis, rads) * Matrix;
        InverseMatrix *= Matrix4x4.CreateFromAxisAngle(axis, -rads);

        updateShaderMatrix();
    }

    public void LoadIdentity()
    {
        EnsureInitialized();
        Matrix = Matrix4x4.Identity;
        InverseMatrix = Matrix4x4.Identity;
        updateShaderMatrix();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ScreenToLocal(Vector2 screenPos)
    {
        return Vector2.Transform(screenPos, InverseMatrix);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ScreenToLocalDirection(Vector2 screenDelta)
    {
        return Vector2.TransformNormal(screenDelta, InverseMatrix);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsPoint(Vector2 screenPos, Vector2 size)
    {
        var localPos = ScreenToLocal(screenPos);
        return localPos.X >= 0 && localPos.X <= size.X &&
               localPos.Y >= 0 && localPos.Y <= size.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsPoint(Vector2 screenPos, Vector2 offset, Vector2 size)
    {
        var localPos = ScreenToLocal(screenPos);
        return localPos.X >= offset.X && localPos.X <= offset.X + size.X &&
               localPos.Y >= offset.Y && localPos.Y <= offset.Y + size.Y;
    }

    private void updateShaderMatrix()
    {
        CurrentShader.SetMatrix4(Shader.TRANSFORM_UNIFORM_NAME, Matrix);
    }

    public void RotateAround(Vector2 pivot, float degrees)
    {
        EnsureInitialized();

        Translate(-pivot.X, -pivot.Y, 0);
        Rotate(degrees, 0, 0, 1);
        Translate(pivot.X, pivot.Y, 0);
    }

    public void Dispose()
    {
        openGlInitialized = false;
        OpenGL.Dispose();
    }
}
