using Synesthesia.Engine.Events;

namespace Synesthesia.Engine.Util.Bindables;

public class BindableScope : IDisposable
{
    private readonly List<IBindable> bindables = [];
    private readonly List<IEventDispatcher> dispatchers = [];
    private readonly List<IEventDispatcher> singleOffDispatchers = [];


    public Bindable<T> Borrow<T>(T defaultValue)
    {
        var bindable = new Bindable<T>(defaultValue);
        bindables.Add(bindable);
        return bindable;
    }

    public EventDispatcher<T> BorrowDispatcher<T>()
    {
        var dispatcher = new EventDispatcher<T>();
        dispatchers.Add(dispatcher);
        return dispatcher;
    }

    public SingleOffEventDispatcher<T> BorrowSingleOffDispatcher<T>()
    {
        var dispatcher = new SingleOffEventDispatcher<T>();
        singleOffDispatchers.Add(dispatcher);
        return dispatcher;
    }

    public void UnregisterDispatcher<T>(IEventDispatcher dispatcher)
    {
        if (!dispatchers.Contains(dispatcher)) return;

        dispatchers.Remove(dispatcher);
        dispatcher.Dispose();
    }

    public void Unregister<T>(IBindable bindable)
    {
        if (!bindables.Contains(bindable)) return;

        bindables.Remove(bindable);
        bindable.Dispose();
    }

    public void Dispose()
    {
        bindables.ForEach(b => b.Dispose());
        bindables.Clear();

        dispatchers.ForEach(b => b.Dispose());
        dispatchers.Clear();

        singleOffDispatchers.ForEach(b => b.Dispose());
        singleOffDispatchers.Clear();

    }
}
