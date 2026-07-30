// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using SDL3;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Graphics.Shaders;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Util.Statistics;
using Synesthesia.Utils.Extensions;
using Shader = Synesthesia.Engine.Graphics.Shader;
using Texture = Synesthesia.Engine.Graphics.Textures.Texture;

namespace Synesthesia.Engine.Platform.Render;

public class GraphicsDevice
{
    public const ClearFlags DEFAULT_CLEAR_FLAGS = ClearFlags.ColorBuffer | ClearFlags.DepthBuffer | ClearFlags.StencilBuffer;

    private const string shader_uniform_texture = "u_texture";
    private const string shader_uniform_use_texture = "u_useTexture";

    public required OpenGLSurface Surface { get; init; }

    public int BackBufferWidth { get; private set; }
    public int BackBufferHeight { get; private set; }
    public ClearFlags ClearFlags = DEFAULT_CLEAR_FLAGS;
    private int stencilDepthStack;

    public Shader CurrentShader { get; private set; } = null!;
    public Shader DefaultShader = null!;
    public Shader StencilShader = null!;

    public int TextureShaderLocation;
    public int UseTextureShaderLocation;

    public Renderer2D Renderer2D = null!;
    //Renderer3D

    public Texture? CurrentTexture { get; private set; }

    public static readonly ConcurrentQueue<Texture> TEXTURE_UPLOAD_QUEUE = new ConcurrentQueue<Texture>();

    private bool openGlInitialized;

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
        Renderer2D = new Renderer2D(this);

        PushViewport();

        var version = OpenGL.GetStringS(GLEnum.Version);
        var shadingLanguageVersion = OpenGL.GetStringS(GLEnum.ShadingLanguageVersion);
        var vendor = OpenGL.GetStringS(GLEnum.Vendor);
        var renderer = OpenGL.GetStringS(GLEnum.Renderer);

        Logger.EmptyLine();
        Logger.Debug("OpenGL Initialized", Logger.Platform);
        Logger.Debug($"- Version:   {version}", Logger.Platform);
        Logger.Debug($"- Vendor:    {vendor}", Logger.Platform);
        Logger.Debug($"- Renderer   {renderer}", Logger.Platform);
        Logger.Debug($"- GLSL:      {shadingLanguageVersion}", Logger.Platform);
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureInitialized()
    {
        if (!openGlInitialized) throw new InvalidOperationException("OpenGL is not initialized yet");
    }

    public void CompileDefaultShaders()
    {
        DefaultShader = new Shader(OpenGL, ShaderSources.DefaultVertex, ShaderSources.DefaultFragment);
        StencilShader = new Shader(OpenGL, ShaderSources.DefaultVertex, ShaderSources.StencilFragment);
        BindShader(DefaultShader);
    }

    public void FlushAllVertexBuffers()
    {
        Renderer2D.FlushVertexBatch();
    }

    public void BindShader(Shader shader)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        if (CurrentShader == shader) return;

        // Flush any pending vertices BEFORE swapping shaders
        if (openGlInitialized)
        {
            Renderer2D.VertexBatch.Flush();
        }

        CurrentShader = shader;
        shader.Use();

        cacheShaderUniformLocations();
        Renderer2D.UpdateShaderMatrix();

        if (CurrentTexture != null)
        {
            CurrentShader.SetInt(TextureShaderLocation, 0);
            CurrentShader.SetInt(UseTextureShaderLocation, 1);
        }
        else
        {
            CurrentShader.SetInt(UseTextureShaderLocation, 0);
        }
    }

    public void UnbindShader()
    {
        ThreadSafety.AssertRunningOnRenderThread();
        BindShader(DefaultShader);

        Renderer2D.UpdateShaderMatrix();
    }

    private void cacheShaderUniformLocations()
    {
        TextureShaderLocation = CurrentShader.GetUniformLocation(shader_uniform_texture);
        UseTextureShaderLocation = CurrentShader.GetUniformLocation(shader_uniform_use_texture);

        Renderer2D.CacheUniformLocations();
    }

    public void BindTexture(Texture? texture)
    {
        ThreadSafety.AssertRunningOnRenderThread();
        if (CurrentTexture == texture) return;

        Renderer2D.FlushVertexBatch();

        CurrentTexture = texture;
        if (texture != null && texture.Bind(OpenGL))
        {
            CurrentShader.SetInt(TextureShaderLocation, 0);
            CurrentShader.SetInt(UseTextureShaderLocation, 1);
        }
        else
        {
            OpenGL.BindTexture(TextureTarget.Texture2D, 0);
            CurrentShader.SetInt(UseTextureShaderLocation, 0);
        }
    }

    public void PushViewport()
    {
        EnsureInitialized();

        SDL.GetWindowSizeInPixels(Surface.WindowHandle, out int w, out int h);
        BackBufferWidth = w;
        BackBufferHeight = h;

        OpenGL.Viewport(0, 0, (uint)w, (uint)h);
    }

    public void BeginDrawing()
    {
        EnsureInitialized();

        DrawStatistics.Reset();
        stencilDepthStack = 0;
        ClearBufferMask mask = ClearBufferMask.None;

        if (ClearFlags.HasFlagFast(ClearFlags.ColorBuffer))
            mask |= ClearBufferMask.ColorBufferBit;
        if (ClearFlags.HasFlagFast(ClearFlags.DepthBuffer))
            mask |= ClearBufferMask.DepthBufferBit;
        if (ClearFlags.HasFlagFast(ClearFlags.StencilBuffer))
            mask |= ClearBufferMask.StencilBufferBit;

        if (mask != ClearBufferMask.None) OpenGL.Clear(mask);

        DrawStatistics.Set(DrawStatistics.Type.TextureUploadQueue, TEXTURE_UPLOAD_QUEUE.Count);

        while (!TEXTURE_UPLOAD_QUEUE.IsEmpty)
        {
            TEXTURE_UPLOAD_QUEUE.TryDequeue(out var texture);
            texture?.Upload(OpenGL);
        }
    }

    public void EndDrawing()
    {
        EnsureInitialized();

        Surface.SwapBuffers();
        BindTexture(null);
        ClearFlags = DEFAULT_CLEAR_FLAGS;
    }

    public void BeginStencil()
    {
        if (stencilDepthStack == 0)
        {
            OpenGL.Enable(GLEnum.StencilTest);
        }

        stencilDepthStack++;
    }

    public void BeginStencilMask()
    {
        OpenGL.Enable(EnableCap.Multisample);
        OpenGL.ColorMask(false, false, false, false);

        OpenGL.StencilFunc(StencilFunction.Equal, stencilDepthStack - 1, 0xFF);
        OpenGL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);

        BindShader(StencilShader);
    }

    public void EndStencilMask()
    {
        OpenGL.ColorMask(true, true, true, true);

        OpenGL.StencilFunc(StencilFunction.Equal, stencilDepthStack, 0xFF);
        OpenGL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

        UnbindShader();
    }

    public void EndStencil()
    {
        stencilDepthStack--;

        if (stencilDepthStack == 0)
        {
            OpenGL.Disable(EnableCap.StencilTest);
        }
        else
        {
            OpenGL.StencilFunc(StencilFunction.Equal, stencilDepthStack, 0xFF);
            OpenGL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
        }

        OpenGL.ColorMask(true, true, true, true);
        UnbindShader();
    }

    public void BeginStencilRestore()
    {
        OpenGL.Enable(EnableCap.Multisample);
        OpenGL.ColorMask(false, false, false, false);

        OpenGL.StencilFunc(StencilFunction.Equal, stencilDepthStack, 0xFF);
        OpenGL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Decr);

        BindShader(StencilShader);
    }
}
