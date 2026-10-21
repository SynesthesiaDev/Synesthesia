// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Resources.Stores;

public interface IHasUnderlyingStore<T>
{
    IResourceStore<T> UnderlyingResourceStore { get; }

}
