// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using Synesthesia.Utils.Extensions;

namespace Synesthesia.Engine.Resources.Stores;

public class FileSystemResourceStore<T>(string basePath, IDictionary<string, Func<Stream, string, T>> loaders) : IResourceStore<T>
{
    private readonly Dictionary<string, Func<Stream, string, T>> loaders = loaders.ToDictionary(k => k.Key.ToLowerInvariant().RemoveSuffix("."), k => k.Value, StringComparer.Ordinal);

    public T Get(string name) => GetOrNull(name) ?? throw new FileNotFoundException($"Resource with name {name} was not found");

    public T? GetOrNull(string name)
    {
        var ext = Path.GetExtension(name).ToLowerInvariant().RemovePrefix(".");
        if (!loaders.TryGetValue(ext, out var loader)) throw new InvalidOperationException($"Resource Loader for type {ext} is not registered to this store");

        var fullPath = Path.Combine(basePath, name);
        if (!File.Exists(fullPath)) return default;

        using var stream = File.OpenRead(fullPath);
        return loader.Invoke(stream, name);
    }

    public IEnumerable<string> List(string prefix = "")
    {
        if (!Directory.Exists(basePath)) return [];

        return Directory.EnumerateFiles(basePath, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(basePath, f))
            .Where(f => f.StartsWith(prefix, StringComparison.Ordinal));
    }

    public void Dispose()
    {
        loaders.Clear();
    }
}
