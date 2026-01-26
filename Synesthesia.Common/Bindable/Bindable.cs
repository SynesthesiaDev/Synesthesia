namespace Common.Bindable;

public class Bindable<T>(T defaultValue) : IDisposable
{
    private T @default = defaultValue;
    private T value = defaultValue;

    public BoundBindable? Bound = null;

    public T Value
    {
        get => value;
        set
        {
            var oldValue = this.value;
            this.value = value;
            listeners.ForEach(listener => listener.Invoke(oldValue, value));
        }
    }

    private List<BindableListener<T>> listeners = [];

    public BindableListener<T> OnValueChange(Action<BindableEvent<T>> func, bool triggerOnce = false)
    {
        var listener = new BindableListener<T>(func);
        listeners.Add(listener);
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
        value = newValue;
    }

    public void Unbind()
    {
        if (Bound == null) return;

        Bound.Bindable.Unregister(Bound.Listener);
        Bound = null;
    }

    public void Unregister(BindableListener<T> listener)
    {
        listeners.Remove(listener);
    }

    public void ResetToDefaultValue()
    {
        Value = @default;
    }

    public void Dispose()
    {
        Unbind();
        listeners.Clear();
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
