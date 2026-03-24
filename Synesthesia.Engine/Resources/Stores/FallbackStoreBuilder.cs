// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;

namespace Synesthesia.Engine.Resources.Stores;

public class FallbackStoreBuilder<T>(IDictionary<string, Func<Stream, T>> resourceLoaders)
{
    private readonly List<IResourceStore<T>> resourceStores = [];

    public FallbackStoreBuilder<T> AddFileSystemStore(string path)
    {
        resourceStores.Add(new FileSystemResourceStore<T>(path, resourceLoaders));
        return this;
    }

    public FallbackStoreBuilder<T> AddAssemblyStream(Assembly assembly)
    {
        resourceStores.Add(new AssemblyResourceStore<T>(assembly, resourceLoaders));
        return this;
    }

    public FallbackResourceStore<T> Build() => new(resourceStores);
}
