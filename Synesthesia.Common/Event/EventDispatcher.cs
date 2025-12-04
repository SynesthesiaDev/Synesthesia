namespace Common.Event;

public class EventDispatcher<T> : IDisposable
{
    private List<EventSubscriber<T>> _eventSubscribers = [];

    public EventSubscriber<T> Subscribe(Action<T> action)
    {
        var eventSubscriber = new EventSubscriber<T>(action);
        _eventSubscribers.Add(eventSubscriber);
        return eventSubscriber;
    }

    public void Dispatch(T value)
    {
        _eventSubscribers.ForEach(_eventSubscriber => _eventSubscriber.action.Invoke(value));
    }

    public void Unsubscribe(EventSubscriber<T> subscriber)
    {
        _eventSubscribers.Remove(subscriber);
    }

    public void UnsubscribeAll()
    {
        _eventSubscribers.Clear();
    }

    public void Dispose()
    {
        UnsubscribeAll();
    }
}