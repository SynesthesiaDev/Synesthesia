// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using SDL3;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Shaders;
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

    public Shader DefaultShader = null!;
    public Shader StencilShader = null!;

    public VertexBatch<Vertex2d> VertexBatch2d = null!;

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

    public int StackDepth => matrixStack.Count;

    public Shader CurrentShader { get; private set; } = null!;

    public Matrix4x4 Matrix { get; private set; } = Matrix4x4.Identity;

    public Matrix4x4 InverseMatrix { get; private set; } = Matrix4x4.Identity;

    private Matrix4x4 projectionMatrix;

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

        openGlInitialized = true;
        Resize(BackBufferWidth, BackBufferHeight);

        var version = OpenGL.GetStringS(GLEnum.Version);
        var shadingLanguageVersion = OpenGL.GetStringS(GLEnum.ShadingLanguageVersion);
        var vendor = OpenGL.GetStringS(GLEnum.Vendor);
        var renderer = OpenGL.GetStringS(GLEnum.Renderer);

        VertexBatch2d = new VertexBatch<Vertex2d>(gl);

        Console.WriteLine(string.Empty);
        Logger.Debug("OpenGL Initialized", Logger.Platform);
        Logger.Debug($"- Version:   {version}", Logger.Platform);
        Logger.Debug($"- Vendor:    {vendor}", Logger.Platform);
        Logger.Debug($"- Renderer   {renderer}", Logger.Platform);
        Logger.Debug($"- GLSL:      {shadingLanguageVersion}", Logger.Platform);
    }

    public void DrawQuad(Vector2 position, Vector2 size, uint packedColor, RectangleF? textureCoord = null)
    {
        var v0 = position;
        var v1 = position with { Y = position.Y + size.Y };
        var v2 = position + size;
        var v3 = position with { X = position.X + size.X };

        v0 = Vector2.Transform(v0, Matrix);
        v1 = Vector2.Transform(v1, Matrix);
        v2 = Vector2.Transform(v2, Matrix);
        v3 = Vector2.Transform(v3, Matrix);

        var tex = textureCoord ?? new Rectangle(0, 0, 1, 1);

        VertexBatch2d.PushVertex(new Vertex2d(v0, new Vector2(tex.Left, tex.Top), packedColor));
        VertexBatch2d.PushVertex(new Vertex2d(v1, new Vector2(tex.Left, tex.Bottom), packedColor));
        VertexBatch2d.PushVertex(new Vertex2d(v2, new Vector2(tex.Right, tex.Bottom), packedColor));
        VertexBatch2d.PushVertex(new Vertex2d(v3, new Vector2(tex.Right, tex.Top), packedColor));
    }

    public void CompileDefaultShaders()
    {
        DefaultShader = new Shader(OpenGL, ShaderSources.DefaultVertex, ShaderSources.DefaultFragment);
        StencilShader = new Shader(OpenGL, ShaderSources.DefaultVertex, ShaderSources.StencilFragment);
        BindShader(DefaultShader);
    }

    public void BindShader(Shader shader)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        if (CurrentShader == shader) return;

        CurrentShader = shader;
        shader.Use();
        updateShaderMatrix();
    }

    public void UnbindShader()
    {
        ThreadSafety.AssertRunningOnRenderThread();
        BindShader(DefaultShader);
        updateShaderMatrix();
    }

    public void Resize(int width, int height)
    {
        EnsureInitialized();
        pushViewport();
    }

    private void pushViewport()
    {
        SDL.GetWindowSizeInPixels(Surface.WindowHandle, out int w, out int h);
        BackBufferWidth = w;
        BackBufferHeight = h;

        projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, w, h, 0, -1, 1);

        OpenGL.Viewport(0, 0, (uint)w, (uint)h);
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

        OpenGL.Disable(EnableCap.CullFace);
        OpenGL.Disable(EnableCap.DepthTest);

        pushViewport();

        LoadIdentity();
        updateShaderMatrix();
    }

    public void EndDrawing()
    {
        EnsureInitialized();

        VertexBatch2d.Flush();

        Surface.SwapBuffers();
        //TODO unbind texture
        DrawStatistics.Reset();

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
        CurrentShader.SetMatrix4(Shader.TRANSFORM_UNIFORM_NAME, Matrix * projectionMatrix);
    }

    public void RotateAround(Vector2 pivot, float degrees)
    {
        EnsureInitialized();

        Translate(-pivot.X, -pivot.Y, 0);
        Rotate(degrees, 0, 0, 1);
        Translate(pivot.X, pivot.Y, 0);
    }

    public void BeginStencil()
    {
        OpenGL.Clear(ClearBufferMask.StencilBufferBit);
        OpenGL.Enable(GLEnum.StencilTest);
    }

    public void BeginStencilMask()
    {
        OpenGL.Enable(EnableCap.Multisample);
        OpenGL.ColorMask(false, false, false, false);
        OpenGL.StencilFunc(StencilFunction.Always, 1, 0x0FF);
        OpenGL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

        BindShader(StencilShader);
    }

    public void EndStencilMask()
    {
        OpenGL.StencilFunc(StencilFunction.Equal, 1, 0x0FF);
        OpenGL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
        OpenGL.ColorMask(true, true, true, true);

        UnbindShader();
    }

    public void EndStencil()
    {
        OpenGL.Disable(EnableCap.StencilTest);
    }

    public void Dispose()
    {
        openGlInitialized = false;
        OpenGL.Dispose();
    }


}
