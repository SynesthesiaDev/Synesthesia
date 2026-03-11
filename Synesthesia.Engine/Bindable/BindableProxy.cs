// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Events;
using SynesthesiaUtil.Types;

namespace Synesthesia.Engine.Bindable;

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
            foreach (var subscriber in subscriberList)
            {
                dispatcher.Unsubscribe(subscriber);
            }
        }

        eventDispatchers.Clear();
    }

    public bool IsDisposed { get; set; }
}
