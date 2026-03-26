// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Resources.Stores;

public class ResourceStoreBuilder<T>
{
    private IDictionary<string, Func<Stream, string, T>> innerLoaders = new Dictionary<string, Func<Stream, string, T>>(StringComparer.Ordinal);
    private FallbackResourceStore<T>? fallbackResourceStore;
    private bool cached;
    private bool async;
    private bool deferred;

    private IResourceStore<T>? manualStore;

    public ResourceStoreBuilder<T> AddLoaders(IDictionary<string, Func<Stream, string, T>> loaders)
    {
        innerLoaders = loaders;
        return this;
    }

    public ResourceStoreBuilder<T> AddFallback(Action<FallbackStoreBuilder<T>> fallback)
    {
        var builder = new FallbackStoreBuilder<T>(innerLoaders);
        fallback.Invoke(builder);
        fallbackResourceStore = builder.Build();

        return this;
    }

    public ResourceStoreBuilder<T> AddManual(IResourceStore<T> resourceStore)
    {
        manualStore = resourceStore;
        return this;
    }

    public ResourceStoreBuilder<T> MakeCached()
    {
        cached = true;
        return this;
    }

    public ResourceStoreBuilder<T> MakeDeferred()
    {
        deferred = true;
        return this;
    }

    public ResourceStoreBuilder<T> MakeAsync()
    {
        async = true;
        return this;
    }

    public IResourceStore<T> Build()
    {
        var baseStore = manualStore ?? fallbackResourceStore ?? throw new InvalidOperationException("Neither manual store or fallback store was added");
        var cachedStore = cached ? new CachedResourceStore<T>(baseStore) : baseStore;
        var asyncStore = async ? new AsyncResourceStore<T>(cachedStore) : cachedStore;
        return deferred ? new DeferredResourceStore<T>(asyncStore) : asyncStore;
    }
}
