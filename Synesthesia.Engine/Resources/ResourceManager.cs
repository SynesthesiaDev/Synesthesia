using System.Reflection;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Resources;

public static class ResourceManager
{
    private static Dictionary<string, object> _cachedResources = new();
    private static Dictionary<string, Func<Stream, object>> _loaderRegistry = new();

    public static void RegisterLoader(string extension, Func<Stream, object> loaderFunction)
    {
        var normalizedExt = extension.ToLowerInvariant().RemovePrefix(".");
        _loaderRegistry[normalizedExt] = loaderFunction;
    }

    public static void Cache(Assembly assembly, string resourceName)
    {
        if (_cachedResources.ContainsKey(resourceName)) return;

        var extension = Path.GetExtension(resourceName).ToLowerInvariant().RemovePrefix(".");
        if (!_loaderRegistry.TryGetValue(extension, out var loaderFunction)) throw new NotSupportedException($"No resource loader registered for type '{extension}'");

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new FileNotFoundException($"Embedded resource '{resourceName}' does not exist!");

        var processedResource = loaderFunction.Invoke(stream);
        _cachedResources.Add(resourceName, processedResource);
    }

    public static int CacheAll(Assembly assembly)
    {
        var list = assembly.GetManifestResourceNames().ToList();
        list.ForEach(resourceName => Cache(assembly, resourceName));
        return list.Count;
    }

    public static void RemoveFromCache(string resourceName)
    {
        _cachedResources.Remove(resourceName);
    }

    public static void ClearCache()
    {
        _cachedResources.Clear();
    }

    public static T Get<T>(string name)
    {
        if (!_cachedResources.TryGetValue(name, out object resource)) throw new KeyNotFoundException($"Resource '{name}' not found in cache.");

        return (T)resource;
    }
}