using Synesthesia.Engine.Events;

namespace Synesthesia.Engine.Util.Bindables;

public class BindableScope : IDisposable
{
    private readonly List<IBindable> bindables = [];
    private readonly List<IEventDispatcher> dispatchers = [];
    private readonly List<IEventDispatcher> singleOffDispatchers = [];

    public BindableScope()
    {
        EngineStatistics.BINDABLE_SCOPES.Increment();
    }

    public Bindable<T> Borrow<T>(T defaultValue)
    {
        var bindable = new Bindable<T>(defaultValue);
        bindables.Add(bindable);
        EngineStatistics.BINDABLE_SCOPE_BORROWS.Increment();
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
        EngineStatistics.BINDABLE_SCOPE_BORROWS.Decrement();
    }

    public void Dispose()
    {
        bindables.ForEach(b => b.Dispose());
        EngineStatistics.BINDABLE_SCOPE_BORROWS.Update(current => current - bindables.Count);
        bindables.Clear();

        dispatchers.ForEach(b => b.Dispose());
        EngineStatistics.DISPATCHERS_BORROWED.Update(current => current - dispatchers.Count);
        dispatchers.Clear();

        singleOffDispatchers.ForEach(b => b.Dispose());
        EngineStatistics.DISPATCHERS_BORROWED.Update(current => current - singleOffDispatchers.Count);
        singleOffDispatchers.Clear();

        EngineStatistics.BINDABLE_SCOPES.Decrement();
    }
}
