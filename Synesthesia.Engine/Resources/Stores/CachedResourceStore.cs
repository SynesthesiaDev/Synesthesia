// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Resources.Stores;

public class CachedResourceStore<T>(IResourceStore<T> underlyingStore) : IResourceStore<T>, IHasUnderlyingStore<T>, IResolvesGraphicsDeferred
{
    private readonly Dictionary<string, T> cache = new(StringComparer.Ordinal);

    public IResourceStore<T> UnderlyingResourceStore => underlyingStore;

    public T Get(string name) => GetOrNull(name) ?? throw new FileNotFoundException($"Resource with name {name} was not found");

    public T? GetOrNull(string name)
    {
        if (cache.TryGetValue(name, out var cached)) return cached;
        var value = underlyingStore.GetOrNull(name);
        if (value != null)
        {
            cache[name] = value;
            EngineStatistics.Increment(EngineStatistics.Type.CachedResourceStoreSize);
        }

        return value;
    }

    public IEnumerable<string> List(string prefix = "") => underlyingStore.List(prefix);

    public void Dispose()
    {
        EngineStatistics.Set(EngineStatistics.Type.CachedResourceStoreSize, EngineStatistics.Get(EngineStatistics.Type.CachedResourceStoreSize) - cache.Count);
        cache.Clear();
        underlyingStore.Dispose();
    }

    public void ResolveGraphicsDeferred(GraphicsDevice graphicsDevice)
    {
        if (underlyingStore is IResolvesGraphicsDeferred deferred)
        {
            deferred.ResolveGraphicsDeferred(graphicsDevice);
        }
    }

}
