using System.Buffers;
using System.Text;
using Codon.Buffer;
using Common.Logger;
using Raylib_cs;
using Synesthesia.Engine.Audio;
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

    public static object LoadVertexShader(Stream stream)
    {
        var text = LoadText(stream) as string;
        return Raylib.LoadShaderFromMemory(text, null);
    }

    public static object LoadFragmentShader(Stream stream)
    {
        var text = LoadText(stream) as string;
        var shader = Raylib.LoadShaderFromMemory(null, text);

        if (shader.Id > 0) return shader;

        var ex = new Exception("Fragment shader failed to load");
        Logger.Exception(ex, Logger.Render);
        throw ex;

    }

    public static object LoadFont(Stream stream)
    {
        var array = LoadByteArray(stream) as byte[];
        return Unsafe.LoadFontFromMemory(array!);
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
        var array = LoadByteArray(stream) as byte[];
        return BinaryBuffer.FromArray(array!);
    }

    public static object LoadAudioSample(Stream stream)
    {
        var buffer = LoadBinaryBuffer(stream) as BinaryBuffer;
        return new AudioSample(buffer!);
    }
}
