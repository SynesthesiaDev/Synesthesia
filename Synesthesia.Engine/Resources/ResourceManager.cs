using System.Reflection;
using Common.Logger;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Resources;

public static class ResourceManager
{
    private static Dictionary<string, CachedResource> _cachedResources = new();
    private static Dictionary<string, UnresolvedResource> _unresolved = new();
    private static Dictionary<string, Func<Stream, object>> _loaderRegistry = new();
    private static List<string> Unresolvable = [];

    public static int CachedSize => _cachedResources.Count;

    public static int UnresolvedSize => _cachedResources.Count;

    public static int Size => CachedSize + UnresolvedSize;

    public static void RegisterLoader(string extension, Func<Stream, object> loaderFunction, bool unresolved = false)
    {
        var normalizedExt = extension.ToLowerInvariant().RemovePrefix(".");
        _loaderRegistry[normalizedExt] = loaderFunction;
        if (unresolved) Unresolvable.Add(extension);
    }

    public static void Cache(Assembly assembly, string resourceName)
    {
        if (_cachedResources.ContainsKey(resourceName)) return;

        var extension = Path.GetExtension(resourceName).ToLowerInvariant().RemovePrefix(".");
        if (!_loaderRegistry.TryGetValue(extension, out var loaderFunction)) throw new NotSupportedException($"No resource loader registered for type '{extension}'");

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new FileNotFoundException($"Embedded resource '{resourceName}' does not exist!");

        if (Unresolvable.Contains(extension))
        {
            var byteArray = ResourceLoaders.LoadByteAray(stream) as byte[];
            var unresolvedResource = new UnresolvedResource(resourceName, extension, byteArray!, loaderFunction);
            _unresolved.Add(resourceName, unresolvedResource);
        }
        else
        {
            var processedResource = loaderFunction.Invoke(stream);
            var resource = new CachedResource(processedResource, resourceName, extension);
            _cachedResources.Add(resourceName, resource);
        }
    }

    public static void ResolveAll(string extension)
    {
        var sanitized = Path.GetExtension(extension).ToLowerInvariant().RemovePrefix(".");
        Unresolvable.Remove(sanitized);
        var size = _unresolved.Count;
        foreach (var unresolvedResource in _unresolved.ToList())
        {
            var resolved = unresolvedResource.Value.Resolve<object>();
            var resolvedName = unresolvedResource.Value.Name;
            var resource = new CachedResource(resolved, resolvedName, sanitized);
            _cachedResources.Add(resolvedName, resource);
            _unresolved.Remove(unresolvedResource.Key);
        }

        Logger.Verbose($"Resolved {size} resource(s) of type '{extension}'", Logger.IO);
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

    public static void RemoveFromCache(CachedResource resource)
    {
        _cachedResources.Remove(resource.Name);
    }

    public static void RemoveFromCache(UnresolvedResource resource)
    {
        _unresolved.Remove(resource.Name);
    }

    public static void ClearCache()
    {
        _cachedResources.Clear();
        _unresolved.Clear();
        _loaderRegistry.Clear();
        Unresolvable.Clear();
    }

    public static UnresolvedResource GetUnresolved(string name)
    {
        return _unresolved.TryGetValue(name, out var unresolved) ? unresolved : throw new KeyNotFoundException($"Resource '{name}' not found in unresolved cache.");
    }

    public static T Get<T>(string name)
    {
        if (_unresolved.ContainsKey(name))
        {
            var message = $"Tried to get unresolved resource {name}";
            Logger.Error(message);
            throw new InvalidDataException(message);
        }

        return _cachedResources.TryGetValue(name, out var unresolved) ? (T)unresolved.Value : throw new KeyNotFoundException($"Resource '{name}' not found in cache.");
    }
}