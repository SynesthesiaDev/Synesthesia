// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Events;
using Synesthesia.Engine.Pooling;

namespace Synesthesia.Engine.Bindable;

public interface IEventDispatcher : IDisposable, IPooledObject
{
    void Unsubscribe(IEventSubscriber subscriber);

    public bool IsDisposed { get; internal set; }

}
