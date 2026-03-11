// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Bindable;

public interface IBindable : IDisposable
{
    static readonly BindableEventSource GLOBAL_EVENT_SOURCE = new();

    public bool IsDisposed { get; internal set; }

}
