// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Platform.Render;

namespace Synesthesia.Engine.Resources.Stores;

public class BuiltInResourceStoreBuilder<T>
{
    private readonly Dictionary<string, T> items = [];
    private readonly Dictionary<string, Func<GraphicsDevice, T>> graphicsDeferred = [];

    public BuiltInResourceStore<T> Build()
    {
        var store = new BuiltInResourceStore<T>();

        foreach (var (key, item) in items) store.Add(key, item);
        foreach (var (key, dItem) in graphicsDeferred) store.AddDeferred(key, dItem);

        return store;
    }

    public void Add(string name, T item)
    {
        if (!items.TryAdd(name, item)) throw new InvalidOperationException($"Resource with name {name} already exists");
    }

    public void AddGraphicsDeferred(string name, Func<GraphicsDevice, T> factory)
    {
        if (!graphicsDeferred.TryAdd(name, factory)) throw new InvalidOperationException($"Resource with name {name} already exists");
    }
}
