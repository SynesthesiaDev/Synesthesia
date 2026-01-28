using Common.Event;
using Common.Statistics;

namespace Common.Bindable;

public class BindablePool : IDisposable
{
    private List<IBindable> bindables = [];
    private List<IEventDispatcher> dispatchers = [];
    private List<IEventDispatcher> singleOffDispatchers = [];

    public BindablePool()
    {
        EngineStatistics.BINDABLE_POOLS.Increment();
    }

    public Bindable<T> Borrow<T>(T defaultValue)
    {
        var bindable = new Bindable<T>(defaultValue);
        bindables.Add(bindable);
        EngineStatistics.BINDABLES_BORROWED.Increment();
        return bindable;
    }

    public EventDispatcher<T> BorrowDispatcher<T>()
    {
        var dispatcher = new EventDispatcher<T>();
        dispatchers.Add(dispatcher);
        EngineStatistics.DISPATCHERS_BORROWED.Increment();
        return dispatcher;
    }

    public SingleOffEventDispatcher<T> BorrowSingleOffDispatcher<T>()
    {
        var dispatcher = new SingleOffEventDispatcher<T>();
        singleOffDispatchers.Add(dispatcher);
        EngineStatistics.DISPATCHERS_BORROWED.Increment();
        return dispatcher;
    }

    public void UnregisterDispatcher<T>(IEventDispatcher dispatcher)
    {
        if (!dispatchers.Contains(dispatcher)) return;

        dispatchers.Remove(dispatcher);
        dispatcher.Dispose();
        EngineStatistics.DISPATCHERS_BORROWED.Decrement();
    }

    public void Unregister<T>(IBindable bindable)
    {
        if (!bindables.Contains(bindable)) return;

        bindables.Remove(bindable);
        bindable.Dispose();
        EngineStatistics.BINDABLES_BORROWED.Decrement();
    }

    public void Dispose()
    {
        bindables.ForEach(b => b.Dispose());
        EngineStatistics.BINDABLES_BORROWED.Update(current => current - bindables.Count);
        bindables.Clear();

        dispatchers.ForEach(b => b.Dispose());
        EngineStatistics.DISPATCHERS_BORROWED.Update(current => current - dispatchers.Count);
        dispatchers.Clear();

        singleOffDispatchers.ForEach(b => b.Dispose());
        EngineStatistics.DISPATCHERS_BORROWED.Update(current => current - singleOffDispatchers.Count);
        singleOffDispatchers.Clear();

        EngineStatistics.BINDABLE_POOLS.Decrement();
    }
}
