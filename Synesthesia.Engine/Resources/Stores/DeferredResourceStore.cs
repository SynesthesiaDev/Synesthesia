// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Resources.Stores;

public class DeferredResourceStore<T>(IResourceStore<T> underlyingStore) : IResourceStore<T>
{
    private bool ready;

    public T Get(string name)
    {
        if (!ready) throw new InvalidOperationException($"Deferred Resource Store for type {typeof(T).Name} is not ready yet");
        return GetOrNull(name) ?? throw new FileNotFoundException($"Resource with name {name} was not found");
    }

    public T? GetOrNull(string name)
    {
        return !ready ? default : underlyingStore.GetOrNull(name);
    }

    public IEnumerable<string> List(string prefix = "")
    {
        return underlyingStore.List();
    }

    public void Unlock()
    {
        ready = true;
    }

    public void Dispose()
    {
        underlyingStore.Dispose();
    }
}
