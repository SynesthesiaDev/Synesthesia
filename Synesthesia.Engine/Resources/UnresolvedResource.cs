namespace Synesthesia.Engine.Resources;

public class UnresolvedResource(string name, string extension, byte[] data, Func<Stream, object> loaderFunction)
{
    public string Name { get; init; } = name;
    public string Extension { get; init; } = extension;
    public byte[] RawData { get; init; } = data;
    public Func<Stream, object> LoaderFunction { get; init; } = loaderFunction;

    public T Resolve<T>()
    {
        return (T)LoaderFunction.Invoke(new MemoryStream(RawData));
    }
}