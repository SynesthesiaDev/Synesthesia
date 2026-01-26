using System.Text;
using Codon.Buffer;
using Common.Logger;
using Raylib_cs;
using Synesthesia.Engine.Audio;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Resources;

public static class ResourceLoaders
{
    public static object LoadText(Stream stream)
    {
        if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return reader.ReadToEnd();
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
        var byteArr = LoadByteAray(stream) as byte[];
        return Unsafe.LoadFontFromMemory(byteArr!);
    }

    public static object LoadByteAray(Stream stream)
    {
        if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public static object LoadBinaryBuffer(Stream stream)
    {
        var array = LoadByteAray(stream) as byte[];
        return BinaryBuffer.FromArray(array!);
    }

    public static object LoadAudioSample(Stream stream)
    {
        var buffer = LoadBinaryBuffer(stream) as BinaryBuffer;
        return new AudioSample(buffer!);
    }
}
