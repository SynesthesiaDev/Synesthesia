// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Platform.Render;

namespace Synesthesia.Engine.Resources.Stores;

public class BuiltInResourceStore<T> : IResourceStore<T>, IResolvesGraphicsDeferred
{
    private readonly Dictionary<string, T> items = [];
    private readonly Dictionary<string, Func<GraphicsDevice, T>> graphicsDeferred = [];

    public void Add(string name, T item)
    {
        if (!items.TryAdd(name, item)) throw new InvalidOperationException($"Resource with name {name} already exists");
    }

    public void AddDeferred(string name, Func<GraphicsDevice, T> factory)
    {
        if (!graphicsDeferred.TryAdd(name, factory)) throw new InvalidOperationException($"Resource with name {name} already exists");
    }

    public T Get(string name) => GetOrNull(name) ?? throw new FileNotFoundException($"Resource with name {name} was not found");

    public T? GetOrNull(string name) => items.GetValueOrDefault(name);

    public IEnumerable<string> List(string prefix = "") => items.Keys;

    public void Dispose()
    {
        items.Clear();
    }

    public void ResolveGraphicsDeferred(GraphicsDevice graphicsDevice)
    {
        foreach (var (key, value) in graphicsDeferred)
        {
            Add(key, value.Invoke(graphicsDevice));
        }
    }
}
