// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Resources;

public interface IResourceStore<T> : IDisposable
{
    T Get(string name);

    T? GetOrNull(string name);

    IEnumerable<string> List(string prefix = "");
}
