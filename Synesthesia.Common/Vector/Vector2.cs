namespace Common.Vector;

public class Vector2(int x, int y)
{
    public readonly int X = x;
    public readonly int Y = y;

    public Vector2f ToVector2f()
    {
        return new Vector2f(X, Y);
    }
}