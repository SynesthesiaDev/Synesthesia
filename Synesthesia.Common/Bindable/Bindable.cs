namespace Common.Bindable;

public class Bindable<T>(T DefaultValue) : IDisposable
{
    private T _default = DefaultValue;
    private T _value = DefaultValue;

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
        _listeners.Clear();
    }

    public void TriggerChange()
    {
        Value = Value;
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