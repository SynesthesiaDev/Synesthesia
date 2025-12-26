namespace Synesthesia.Engine.Resources;

public class CachedResource(object value, string name, string extension)
{
    public object Value { get; init; } = value;
    public string Name { get; init; } = name;
    public string Extension { get; init; } = extension;

}