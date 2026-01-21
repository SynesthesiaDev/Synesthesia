using Common.Statistics;

namespace Common.Event;

public class SingleOffEventDispatcher<T> : IDisposable
{
    private List<EventSubscriber<T>> _eventSubscribers = [];
    private T? _dispatchedValue;

    public SingleOffEventDispatcher()
    {
        EngineStatistics.Dispatchers.Increment();
    }

    public void Subscribe(Action<T> action)
    {
        if (_dispatchedValue == null)
        {
            var eventSubscriber = new EventSubscriber<T>(action);
            _eventSubscribers.Add(eventSubscriber);
        }
        else
        {
            action.Invoke(_dispatchedValue!);
        }
    }

    public void Dispatch(T value)
    {
        if (_dispatchedValue != null) throw new InvalidOperationException("This event dispatcher has already value dispatched!");
        _dispatchedValue = value;
        _eventSubscribers.ForEach(_eventSubscriber => _eventSubscriber.action.Invoke(value));
        _eventSubscribers.Clear();
    }


    public void Dispose()
    {
        _eventSubscribers.Clear();
        EngineStatistics.Dispatchers.Decrement();
    }
}