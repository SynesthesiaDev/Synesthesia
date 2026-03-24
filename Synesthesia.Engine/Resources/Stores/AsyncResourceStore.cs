// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Util.Future;

namespace Synesthesia.Engine.Resources.Stores;

public class AsyncResourceStore<T>(IResourceStore<T> underlyingStore) : IAsyncResourceStore<T>
{

    private readonly Dictionary<string, CompletableFuture<T>> inflight = new(StringComparer.Ordinal);

    public CompletableFuture<T> GetAsync(string name)
    {
        if (inflight.TryGetValue(name, out var existing)) return existing;

        var future = new CompletableFuture<T>();
        inflight[name] = future;

        Task.Run(() =>
        {
            try
            {
                var result = underlyingStore.GetOrNull(name);
                if (result == null) future.Fail(new FileNotFoundException($"Resource with name {name} was not found"));
                else future.Complete(result);
            }
            catch (Exception exception)
            {
                future.Fail(exception);
            }
            finally
            {
                inflight.Remove(name);
            }
        });

        return future;
    }

    public CompletableFuture<T?> GetOrNullAsync(string name)
    {
        var future = new CompletableFuture<T?>();

        GetAsync(name)
            .Then(result => future.Complete(result))
            .OnFail(_ => future.Complete(default));

        return future;
    }

    public T Get(string name) => underlyingStore.Get(name);

    public T? GetOrNull(string name) => underlyingStore.GetOrNull(name);

    public IEnumerable<string> List(string prefix = "") => underlyingStore.List();

    public void Dispose()
    {
        underlyingStore.Dispose();
        inflight.Clear();
    }

}
