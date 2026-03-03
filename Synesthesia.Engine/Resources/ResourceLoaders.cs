using System.Buffers;
using System.Text;
using Codon.Buffer;
using Raylib_cs;
using Synesthesia.Engine.Audio;
using Synesthesia.Engine.Graphics.Fonts;
using Synesthesia.Engine.Graphics.Shaders;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Resources;

public static class ResourceLoaders
{
    private static byte[] getStreamBytes(Stream stream, out int length)
    {
        if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

        length = (int)stream.Length;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

        int totalRead = 0;
        while (totalRead < length)
        {
            int read = stream.Read(buffer, totalRead, length - totalRead);
            if (read == 0) break;
            totalRead += read;
        }

        return buffer;
    }

    public static object LoadText(Stream stream)
    {
        byte[] buffer = getStreamBytes(stream, out int length);
        try
        {
            return Encoding.UTF8.GetString(buffer.AsSpan(0, length));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static Texture LoadTexture(Stream stream, string ext)
    {
        var bytes = getStreamBytes(stream, out int lenght);
        Texture texture;
        try
        {
            unsafe
            {
                fixed (byte* pData = bytes)
                {
                    var image = Raylib.LoadImageFromMemory(new Utf8Buffer(ext).AsPointer(), pData, lenght);

                    image.Mipmaps = 1;
                    texture = new Texture(image);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }

        return texture;
    }

    public static Shader LoadShader(Stream stream, ShaderType type)
    {
        var text = LoadText(stream) as string;
        return new Shader(text!, type);
    }

    public static Font LoadFont(Stream stream)
    {
        ThreadSafety.AssertRunningOnRenderThread();

        var array = LoadByteArray(stream) as byte[];
        var raylibFont = Unsafe.LoadFontFromMemory(array!);
        return new Font(raylibFont);
    }

    public static object LoadByteArray(Stream stream)
    {
        if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

        byte[] buffer = new byte[stream.Length];
        stream.ReadExactly(buffer);
        return buffer;
    }

    public static object LoadBinaryBuffer(Stream stream)
    {
        var array = getStreamBytes(stream, out var lenght);
        var buffer = BinaryBuffer.FromArray(array!);
        ArrayPool<byte>.Shared.Return(array);

        return buffer;
    }

    public static AudioSample LoadAudioSample(Stream stream)
    {
        var buffer = LoadBinaryBuffer(stream) as BinaryBuffer;
        return new AudioSample(buffer!);
    }
}
