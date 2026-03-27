using Synesthesia.Engine.Util.Pooling;
using Synesthesia.Engine.Util.Statistics;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Util.Bindables;

public class Bindable<T> : IBindable, IPooledObject
{
    private readonly T defaultInternal;
    protected T InternalValue;

    public bool IsDisposed { get; set; }

    public BoundBindable? Bound;

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    public T Value
    {
        get => InternalValue;
        set => Set(value, IBindable.GLOBAL_EVENT_SOURCE);
    }

    public virtual void Set(T newValue, BindableEventSource source)
    {
        var oldValue = InternalValue;
        InternalValue = newValue;
        Listeners.Filter(p => p.Value != source).Keys.ToList().ForEach(listener => listener.Invoke(oldValue, newValue));
    }

    protected readonly Dictionary<BindableListener<T>, BindableEventSource?> Listeners = [];

    public Bindable(T defaultInternalValue)
    {
        defaultInternal = defaultInternalValue;
        InternalValue = defaultInternalValue;
        EngineStatistics.Increment(EngineStatistics.Type.Bindables);
    }

    public BindableListener<T> OnValueChange(Action<BindableEvent<T>> func, bool triggerOnce = false, BindableEventSource? ignoresSource = null)
    {
        var listener = new BindableListener<T>(func);
        Listeners.Add(listener, ignoresSource);
        if (triggerOnce) listener.Invoke(Value, Value);
        return listener;
    }

    public virtual void BindTo(Bindable<T> them)
    {
        if (them == this) throw new InvalidOperationException("Cannot bind to self");
        if (Bound != null) throw new InvalidOperationException("Bindable (self) is already bound");

        Value = them.Value;
        var boundListener = them.OnValueChange(e => Value = e.NewValue);

        Bound = new BoundBindable(them, boundListener);
    }

    public void SetSilently(T newValue)
    {
        InternalValue = newValue;
    }

    public void Unbind()
    {
        if (Bound == null) return;

        Bound.Bindable.Unregister(Bound.Listener);
        Bound = null;
    }

    public void Unregister(BindableListener<T> listener)
    {
        Listeners.Remove(listener);
    }

    public void ResetToDefaultValue()
    {
        Value = defaultInternal;
    }

    public void Dispose()
    {
        Unbind();
        Listeners.Clear();
        IsDisposed = true;
        EngineStatistics.Decrement(EngineStatistics.Type.Bindables);
    }

    public void TriggerChange()
    {
        Value = Value;
    }

    public record BoundBindable(Bindable<T> Bindable, BindableListener<T> Listener);

    public void Reset()
    {
        Unbind();
        Listeners.Clear();
    }

}

public record BindableListener<T>(Action<BindableEvent<T>> Func)
{
    public void Invoke(T oldValue, T newValue)
    {
        Func.Invoke(new BindableEvent<T>(oldValue, newValue));
    }
}

public record BindableEvent<T>(T OldValue, T NewValue);

public class BindableEventSource
{
    public Guid Uuid = Guid.NewGuid();
}
