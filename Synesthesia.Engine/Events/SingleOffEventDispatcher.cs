using Synesthesia.Engine.Bindable;
using Synesthesia.Engine.Pooling;

namespace Synesthesia.Engine.Events;

public class SingleOffEventDispatcher<T> : IEventDispatcher
{
    private readonly List<EventSubscriber<T>> eventSubscribers = [];

    private T? dispatchedValue;

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

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


    public void Clear()
    {
        eventSubscribers.Clear();
    }

    public void Dispose()
    {
        Clear();
        EngineStatistics.DISPATCHERS.Decrement();
        IsDisposed = true;
    }

    public void Unsubscribe(IEventSubscriber subscriber)
    {
        throw new NotSupportedException();
    }

    public bool IsDisposed { get; set; }

    public void Reset()
    {
        Clear();
        dispatchedValue = default;
    }
}
