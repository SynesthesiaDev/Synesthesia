
using Synesthesia.Engine.Bindable;
using Synesthesia.Engine.Pooling;

namespace Synesthesia.Engine.Events;

public class EventDispatcher<T> : IEventDispatcher
{
    private readonly List<EventSubscriber<T>> eventSubscribers = [];

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    public bool IsDisposed { get; set; }

    public EventDispatcher()
    {
        EngineStatistics.DISPATCHERS.Increment();
    }

    public EventSubscriber<T> Subscribe(Action<T> action)
    {
        var eventSubscriber = new EventSubscriber<T>(action);
        eventSubscribers.Add(eventSubscriber);
        return eventSubscriber;
    }

    public void Dispatch(T value)
    {
        eventSubscribers.ForEach(eventSubscriber => eventSubscriber.Action.Invoke(value));
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
        EngineStatistics.DISPATCHERS.Decrement();
        UnsubscribeAll();
        IsDisposed = true;
    }

    public void Reset()
    {
        UnsubscribeAll();
    }
}
