// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Silk.NET.OpenGL;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Util.Exceptions;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Graphics.Textures;

public class Texture : IDisposable
{
    public uint Handle { get; private set; }

    public TextureData TextureData { get; private set; }

    public int Width => TextureData.Width;
    public int Height => TextureData.Height;
    public PixelFormat PixelFormat => TextureData.PixelFormat;

    public bool IsUploaded { get; private set; }

    public bool UploadQueued { get; private set; }

    public bool UploadImmediately { get; }

    private GL? gl;

    public Texture(TextureData textureData, bool uploadImmediately)
    {
        UploadImmediately = uploadImmediately;
        TextureData = textureData;
        IsUploaded = false;

        if (UploadImmediately) EnqueueUpload();
    }

    public void EnqueueUpload()
    {
        OpenGlRenderer.TEXTURE_UPLOAD_QUEUE.Enqueue(this);
        UploadQueued = true;
    }

    public void Upload(GL opengl)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        if (IsUploaded) return;
        if (TextureData.Data.Length == 0) throw new OpenGLException("No pixel data");

        gl = opengl;
        Handle = gl.GenTexture();
        opengl.BindTexture(TextureTarget.Texture2D, Handle);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        unsafe
        {
            fixed (void* ptr = TextureData.Data)
            {
                gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)Width, (uint)Height, 0, TextureData.PixelFormat, PixelType.UnsignedByte, ptr);
            }
        }

        IsUploaded = true;
        UploadQueued = false;
        Logger.Verbose($"Uploaded texture {ToString()}", Logger.Render);
    }

    public bool Bind(GL opengl, TextureUnit unit = TextureUnit.Texture0)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        switch (IsUploaded)
        {
            case false when !UploadQueued:
                EnqueueUpload();
                return false;
            case false:
                return false;
        }

        gl!.ActiveTexture(unit);
        gl.BindTexture(TextureTarget.Texture2D, Handle);
        DrawStatistics.Increment(DrawStatistics.Type.TextureBinds);
        return true;
    }

    public void Dispose()
    {
        if (!IsUploaded || gl == null) return;

        ThreadSafety.AssertRunningOnRenderThread();
        gl.DeleteTexture(Handle);
    }

    public override string ToString()
    {
        return $"Texture(Handle={Handle}, TextureData={TextureData}, IsUploaded={IsUploaded}, UploadQueued={UploadQueued})";
    }

}
