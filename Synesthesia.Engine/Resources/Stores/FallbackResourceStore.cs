// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Logging;

namespace Synesthesia.Engine.Resources.Stores;

public class FallbackResourceStore<T>(IList<IResourceStore<T>> resourceStores) : IResourceStore<T>
{
    public FallbackResourceStore(IEnumerable<IResourceStore<T>> stores) : this(stores.ToList())
    {
    }

    public void Prepend(IResourceStore<T> store) => resourceStores.Insert(0, store);

    public T Get(string name) => GetOrNull(name) ?? throw new FileNotFoundException($"Resource with name {name} was not found in any store");

    public T? GetOrNull(string name)
    {
        try
        {
            foreach (var store in resourceStores)
            {
                var result = store.GetOrNull(name);
                if (result != null) return result;
            }
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, Logger.Io);
        }

        return default;
    }

    public IEnumerable<string> List(string prefix = "") => resourceStores.SelectMany(s => s.List(prefix)).Distinct(StringComparer.Ordinal);


    public void Dispose()
    {
        foreach (var resourceStore in resourceStores.AsEnumerable())
        {
            resourceStore.Dispose();
        }
    }
}
