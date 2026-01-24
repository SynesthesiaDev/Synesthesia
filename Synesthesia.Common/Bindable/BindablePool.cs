using Common.Event;
using Common.Statistics;
using SynesthesiaUtil.Extensions;

namespace Common.Bindable;

public class BindablePool : IDisposable
{
    private List<Bindable<object>> bindables = [];
    private List<EventDispatcher<object>> dispatchers = [];
    private List<SingleOffEventDispatcher<object>> singleOffDispatchers = [];

    public BindablePool()
    {
        EngineStatistics.BINDABLE_POOLS.Increment();
    }

    public Bindable<T> Borrow<T>(T defaultValue)
    {
        var bindable = new Bindable<T>(defaultValue);
        bindables.Add((bindable as Bindable<object>)!);
        EngineStatistics.BINDABLES_BORROWED.Increment();
        return bindable;
    }

    public EventDispatcher<T> BorrowDispatcher<T>()
    {
        var dispatcher = new EventDispatcher<T>();
        dispatchers.Add((dispatcher as EventDispatcher<object>)!);
        EngineStatistics.DISPATCHERS_BORROWED.Increment();
        return dispatcher;
    }

    public SingleOffEventDispatcher<T> BorrowSingleOffDispatcher<T>()
    {
        var dispatcher = new SingleOffEventDispatcher<T>();
        singleOffDispatchers.Add((dispatcher as SingleOffEventDispatcher<object>)!);
        EngineStatistics.DISPATCHERS_BORROWED.Increment();
        return dispatcher;
    }

    public void UnregisterDispatcher<T>(EventDispatcher<T> dispatcher)
    {
        var cast = (dispatcher as EventDispatcher<object>)!;

        if (!dispatchers.Contains(cast)) return;

        dispatchers.Remove(cast);
        cast.Dispose();
        EngineStatistics.DISPATCHERS_BORROWED.Decrement();
    }

    public void Unregister<T>(Bindable<T> bindable)
    {
        var cast = (bindable as Bindable<object>)!;

        if (!bindables.Contains(cast)) return;

        bindables.Remove(cast);
        cast.Dispose();
        EngineStatistics.BINDABLES_BORROWED.Decrement();
    }

    public void Dispose()
    {
        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        bindables.Filter(p => p != null).ForEach(b => b.Dispose());
        dispatchers.Filter(p => p != null).ForEach(b => b.Dispose());
        singleOffDispatchers.Filter(p => p != null).ForEach(b => b.Dispose());
        bindables.Clear();
        dispatchers.Clear();
        singleOffDispatchers.Clear();
        EngineStatistics.BINDABLE_POOLS.Decrement();
    }
}