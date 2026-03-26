// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Silk.NET.OpenGL;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Util.Exceptions;

namespace Synesthesia.Engine.Graphics.Textures;

public class Texture : IDisposable
{
    public uint Handle { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public byte[]? Data { get; private set; }

    public bool IsUploaded { get; private set; }

    public bool UploadQueued { get; private set; }

    public bool UploadImmediately { get; }

    private GL? gl;
    private readonly PixelFormat format;

    public string AssetName { get; }

    public Texture(int width, int height, byte[] data, PixelFormat format, string name, bool uploadImmediately)
    {
        this.format = format;
        UploadImmediately = uploadImmediately;
        Width = width;
        Height = height;
        Data = data;
        AssetName = name;
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
        if (Data == null || Data.Length == 0) throw new OpenGLException("No pixel data");

        gl = opengl;
        Handle = gl.GenTexture();
        opengl.BindTexture(TextureTarget.Texture2D, Handle);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        unsafe
        {
            fixed (void* ptr = Data)
            {
                gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)Width, (uint)Height, 0, format, PixelType.UnsignedByte, ptr);
            }
        }

        IsUploaded = true;
        UploadQueued = false;
        Data = null;
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
        return $"Texture(Handle={Handle}, AssetName={AssetName}, Width={Width}, Height={Height}, IsUploaded={IsUploaded}, UploadQueued={UploadQueued}, Format={format})";
    }
}
