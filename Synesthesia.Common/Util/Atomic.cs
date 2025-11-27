namespace Common.Util;

public class Atomic<T>(T value)
{
    private readonly object _lock = new object();
    private T _value = value;

    public T Value
    {
        get
        {
            lock (_lock)
            {
                return _value;
            }
        }
        set
        {
            lock (_lock)
            {
                _value = value;
            }
        }
    }

    public T Update(Func<T, T> updateFunction)
    {
        lock (_lock)
        {
            _value = updateFunction(_value);
            return _value;
        }
    }
}