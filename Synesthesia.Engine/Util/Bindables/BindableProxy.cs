// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Events;
using Synesthesia.Utils.Types;
using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Util.Bindables;

public class BindableProxy : IBindable
{
    private readonly NestedValueMap<IEventDispatcher, IEventSubscriber> eventDispatchers = [];

    public EventSubscriber<T> Subscribe<T>(EventDispatcher<T> dispatcher, Action<T> action)
    {
        var subscriber = dispatcher.Subscribe(action);
        eventDispatchers.AddValue(dispatcher, subscriber);

        return subscriber;
    }

    public void Dispose()
    {
        foreach (var (dispatcher, subscriberList) in eventDispatchers)
        {
            foreach (ref IEventSubscriber subscriber in CollectionsMarshal.AsSpan(subscriberList))
            {
                dispatcher.Unsubscribe(subscriber);
            }
        }

        eventDispatchers.Clear();
    }

    public bool IsDisposed { get; set; }
}
