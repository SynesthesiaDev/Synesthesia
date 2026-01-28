using Common.Bindable;
using Common.Statistics;

namespace Common.Event;

public class SingleOffEventDispatcher<T> : IEventDispatcher
{
    private List<EventSubscriber<T>> eventSubscribers = [];
    private T? dispatchedValue;

    public SingleOffEventDispatcher()
    {
        EngineStatistics.DISPATCHERS.Increment();
    }

    public void Subscribe(Action<T> action)
    {
        if (dispatchedValue == null)
        {
            var eventSubscriber = new EventSubscriber<T>(action);
            eventSubscribers.Add(eventSubscriber);
        }
        else
        {
            action.Invoke(dispatchedValue!);
        }
    }

    public void Dispatch(T value)
    {
        if (dispatchedValue != null) throw new InvalidOperationException("This event dispatcher has already value dispatched!");
        dispatchedValue = value;
        eventSubscribers.ForEach(eventSubscriber => eventSubscriber.Action.Invoke(value));
        eventSubscribers.Clear();
    }


    public void Dispose()
    {
        eventSubscribers.Clear();
        EngineStatistics.DISPATCHERS.Decrement();
    }
}
