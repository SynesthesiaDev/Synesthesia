// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Logging;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Platform.Render;

public sealed class OpenGlRenderer : IDisposable
{
    private const ClearFlags default_clear_flags = ClearFlags.ColorBuffer | ClearFlags.DepthBuffer | ClearFlags.StencilBuffer;

    public required OpenGLSurface Surface { get; init; }

    private bool openGlInitialized;

    public int BackBufferWidth { get; private set; }

    public int BackBufferHeight { get; private set; }

    public ClearFlags ClearFlags = default_clear_flags;

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

    public void Initialize()
    {
        if(openGlInitialized) throw new InvalidOperationException("OpenGL is already initialized");

        var gl = GL.GetApi(name =>
        {
            var ptr = OpenGLSurface.GetProcAddress(name);
            return ptr;
        });

        OpenGL = gl ?? throw new InvalidOperationException("Silk.NET could not bind to OpenGL");

        BackBufferHeight = Surface.BackBufferHeight;
        BackBufferWidth = Surface.BackBufferWidth;

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

        if(mask != ClearBufferMask.None) OpenGL.Clear(mask);

    }

    public void EndDrawing()
    {
        EnsureInitialized();

        Surface.SwapBuffers();

        ClearFlags = default_clear_flags;
    }

    public void Dispose()
    {
        openGlInitialized = false;
        OpenGL.Dispose();
    }
}
