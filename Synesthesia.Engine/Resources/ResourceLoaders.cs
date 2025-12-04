using System.Text;

namespace Synesthesia.Engine.Resources;

public static class ResourceLoaders
{
    public static object LoadText(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return reader.ReadToEnd();
    }
}