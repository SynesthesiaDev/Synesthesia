using Common.Event;
using Common.Statistics;
using SynesthesiaUtil.Extensions;

namespace Common.Bindable;

public class BindablePool : IDisposable
{
    private List<Bindable<object>> _bindables = [];
    private List<EventDispatcher<object>> _dispatchers = [];

    public BindablePool()
    {
        EngineStatistics.BindablePools.Increment();
    }

    public Bindable<T> Borrow<T>(T defaultValue)
    {
        var bindable = new Bindable<T>(defaultValue);
        _bindables.Add((bindable as Bindable<object>)!);
        EngineStatistics.BindablesBorrowed.Increment();
        return bindable;
    }

    public EventDispatcher<T> BorrowDispatcher<T>()
    {
        var dispatcher = new EventDispatcher<T>();
        _dispatchers.Add((dispatcher as EventDispatcher<object>)!);
        EngineStatistics.DispatchersBorrowed.Increment();
        return dispatcher;
    }

    public void UnregisterDispatcher<T>(EventDispatcher<T> dispatcher)
    {
        var cast = (dispatcher as EventDispatcher<object>)!;

        if (!_dispatchers.Contains(cast)) return;

        _dispatchers.Remove(cast);
        cast.Dispose();
        EngineStatistics.DispatchersBorrowed.Decrement();
    }

    public void Unregister<T>(Bindable<T> bindable)
    {
        var cast = (bindable as Bindable<object>)!;

        if (!_bindables.Contains(cast)) return;

        _bindables.Remove(cast);
        cast.Dispose();
        EngineStatistics.BindablesBorrowed.Decrement();
    }

    public void Dispose()
    {
        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        _bindables.Filter(p => p != null).ForEach(b => b.Dispose());
        _dispatchers.Filter(p => p != null).ForEach(b => b.Dispose());
        _bindables.Clear();
        _dispatchers.Clear();
    }
}