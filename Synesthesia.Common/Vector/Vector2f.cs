namespace Common.Vector;

public class Vector2f(float x, float y)
{
    public readonly float X = x;
    public readonly float Y = y;

    public Vector2 ToVector2()
    {
        return new Vector2((int)X, (int)Y);
    }
}