// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common;

namespace Synesthesia.Engine.Resources;

public interface IAsyncResourceStore<T> : IResourceStore<T>
{
    CompletableFuture<T> GetAsync(string name);

    CompletableFuture<T?> GetOrNullAsync(string name);
}
