namespace Common.Bindable;

public class Bindable<T>(T defaultInternalValue) : IBindable
{
    private T defaultInternal = defaultInternalValue;
    protected T InternalValue = defaultInternalValue;

    public BoundBindable? Bound = null;

    public T Value
    {
        get => InternalValue;
        set
        {
            var oldValue = this.InternalValue;
            this.InternalValue = value;
            Listeners.ForEach(listener => listener.Invoke(oldValue, value));
        }
    }

    protected readonly List<BindableListener<T>> Listeners = [];

    public BindableListener<T> OnValueChange(Action<BindableEvent<T>> func, bool triggerOnce = false)
    {
        var listener = new BindableListener<T>(func);
        Listeners.Add(listener);
        if (triggerOnce) listener.Invoke(Value, Value);
        return listener;
    }

    public virtual void BindTo(Bindable<T> them)
    {
        if (Bound != null) throw new InvalidOperationException("Bindable (self) is already bound");
        // if (them.Bound != null) throw new InvalidOperationException("Bindable (them) is already bound");

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
    }

    public void TriggerChange()
    {
        Value = Value;
    }

    public record BoundBindable(Bindable<T> Bindable, BindableListener<T> Listener);
}

public record BindableListener<T>(Action<BindableEvent<T>> Func)
{
    public void Invoke(T oldValue, T newValue)
    {
        Func.Invoke(new BindableEvent<T>(oldValue, newValue));
    }
}

public record BindableEvent<T>(T OldValue, T NewValue);
