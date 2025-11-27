namespace Common.Util;

public class AtomicInt(int initialValue) : Atomic<int>(initialValue)
{
    public int Increment()
    {
        return Update(Value => Value + 1);
    }

    public int Decrement()
    {
        return Update(Value => Value - 1);
    }
}