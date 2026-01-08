namespace Common.Bindable;

public class Bindable<T>(T DefaultValue) : IDisposable
{
    private T _default = DefaultValue;
    private T _value = DefaultValue;

    public BoundBindable? Bound = null;

    public T Value
    {
        get => _value;
        set
        {
            var oldValue = _value;
            _value = value;
            _listeners.ForEach(listener => listener.Invoke(oldValue, value));
        }
    }

    private List<BindableListener<T>> _listeners = [];

    public BindableListener<T> OnValueChange(Action<BindableEvent<T>> func, bool triggerOnce = false)
    {
        var listener = new BindableListener<T>(func);
        _listeners.Add(listener);
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
        _value = newValue;
    }

    public void Unbind()
    {
        if (Bound == null) return;

        Bound.bindable.Unregister(Bound.listener);
        Bound = null;
    }

    public void Unregister(BindableListener<T> listener)
    {
        _listeners.Remove(listener);
    }

    public void ResetToDefaultValue()
    {
        Value = _default;
    }

    public void Dispose()
    {
        Unbind();
        _listeners.Clear();
    }

    public void TriggerChange()
    {
        Value = Value;
    }

    public record BoundBindable(Bindable<T> bindable, BindableListener<T> listener);
}

public record BindableListener<T>(Action<BindableEvent<T>> Func)
{
    public void Invoke(T oldValue, T newValue)
    {
        Func.Invoke(new BindableEvent<T>(oldValue, newValue));
    }
}

public record BindableEvent<T>(T OldValue, T NewValue);