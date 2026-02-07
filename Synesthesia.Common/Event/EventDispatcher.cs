using Common.Bindable;
using Common.Statistics;

namespace Common.Event;

public class EventDispatcher<T> : IEventDispatcher
{
    private readonly List<EventSubscriber<T>> eventSubscribers = [];

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
        EngineStatistics.DISPATCHERS.Decrement();
        UnsubscribeAll();
    }
}
