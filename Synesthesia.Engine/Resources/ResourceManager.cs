using System.Reflection;
using Common.Logger;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Resources;

public static class ResourceManager
{
    private static Dictionary<string, CachedResource> cachedResources = new();
    private static Dictionary<string, UnresolvedResource> unresolved = new();
    private static Dictionary<string, Func<Stream, object>> loaderRegistry = new();
    private static List<string> unresolvable = [];

    public static int CachedSize => cachedResources.Count;

    public static int UnresolvedSize => cachedResources.Count;

    public static int Size => CachedSize + UnresolvedSize;

    public static void RegisterLoader(string extension, Func<Stream, object> loaderFunction, bool unresolved = false)
    {
        var normalizedExt = extension.ToLowerInvariant().RemovePrefix(".");
        loaderRegistry[normalizedExt] = loaderFunction;
        if (unresolved) unresolvable.Add(extension);
    }

    public static void Cache(Assembly assembly, string resourceName)
    {
        if (cachedResources.ContainsKey(resourceName)) return;

        var extension = Path.GetExtension(resourceName).ToLowerInvariant().RemovePrefix(".");
        if (!loaderRegistry.TryGetValue(extension, out var loaderFunction))
        {
            Logger.Warning($"No resource loader registered for type {extension}, skipping");
            return;
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new FileNotFoundException($"Embedded resource '{resourceName}' does not exist!");

        if (unresolvable.Contains(extension))
        {
            var byteArray = ResourceLoaders.LoadByteAray(stream) as byte[];
            var unresolvedResource = new UnresolvedResource(resourceName, extension, byteArray!, loaderFunction);
            unresolved.Add(resourceName, unresolvedResource);
        }
        else
        {
            var processedResource = loaderFunction.Invoke(stream);
            var resource = new CachedResource(processedResource, resourceName, extension);
            cachedResources.Add(resourceName, resource);
        }
    }

    public static void ResolveAll(string extension)
    {
        var sanitized = Path.GetExtension(extension).ToLowerInvariant().RemovePrefix(".");
        unresolvable.Remove(sanitized);
        var size = unresolved.Count;
        foreach (var unresolvedResource in unresolved.ToList())
        {
            var resolved = unresolvedResource.Value.Resolve<object>();
            var resolvedName = unresolvedResource.Value.Name;
            var resource = new CachedResource(resolved, resolvedName, sanitized);
            cachedResources.Add(resolvedName, resource);
            unresolved.Remove(unresolvedResource.Key);
        }

        Logger.Verbose($"Resolved {size} resource(s) of type '{extension}'", Logger.Io);
    }

    public static int CacheAll(Assembly assembly)
    {
        var list = assembly.GetManifestResourceNames().ToList();
        list.ForEach(resourceName => Cache(assembly, resourceName));
        return list.Count;
    }

    public static void RemoveFromCache(string resourceName)
    {
        cachedResources.Remove(resourceName);
    }

    public static void RemoveFromCache(CachedResource resource)
    {
        cachedResources.Remove(resource.Name);
    }

    public static void RemoveFromCache(UnresolvedResource resource)
    {
        unresolved.Remove(resource.Name);
    }

    public static void ClearCache()
    {
        cachedResources.Clear();
        unresolved.Clear();
        loaderRegistry.Clear();
        unresolvable.Clear();
    }

    public static UnresolvedResource GetUnresolved(string name)
    {
        return ResourceManager.unresolved.TryGetValue(name, out var unresolved) ? unresolved : throw new KeyNotFoundException($"Resource '{name}' not found in unresolved cache.");
    }

    public static T Get<T>(string name)
    {
        if (ResourceManager.unresolved.ContainsKey(name))
        {
            var message = $"Tried to get unresolved resource {name}";
            Logger.Error(message);
            throw new InvalidDataException(message);
        }

        return cachedResources.TryGetValue(name, out var unresolved) ? (T)unresolved.Value : throw new KeyNotFoundException($"Resource '{name}' not found in cache.");
    }
}
