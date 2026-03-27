using Synesthesia.Engine.Util.Bindables;
using Synesthesia.Engine.Util.Pooling;
using System.Runtime.InteropServices;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Events;

public class EventDispatcher<T> : IEventDispatcher
{
    private readonly List<EventSubscriber<T>> eventSubscribers = [];

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    public bool IsDisposed { get; set; }

    public EventDispatcher()
    {
        EngineStatistics.Increment(EngineStatistics.Type.Dispatchers);
    }

    public EventSubscriber<T> Subscribe(Action<T> action)
    {
        var eventSubscriber = new EventSubscriber<T>(action);
        eventSubscribers.Add(eventSubscriber);
        return eventSubscriber;
    }

    public void Dispatch(T value)
    {
        foreach (ref EventSubscriber<T> subscriber in CollectionsMarshal.AsSpan(eventSubscribers))
        {
            subscriber.Action.Invoke(value);
        }
    }

    public void Unsubscribe(IEventSubscriber subscriber)
    {
        eventSubscribers.Remove((subscriber as EventSubscriber<T>)!);
    }

    public void UnsubscribeAll()
    {
        eventSubscribers.Clear();
    }

    public void Dispose()
    {
        if(IsDisposed) return;
        EngineStatistics.Decrement(EngineStatistics.Type.Dispatchers);
        UnsubscribeAll();
        IsDisposed = true;
    }

    public void Reset()
    {
        UnsubscribeAll();
    }
}
