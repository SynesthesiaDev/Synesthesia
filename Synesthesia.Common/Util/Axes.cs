namespace Common.Util;

[Flags]
public enum Axes
{
    None = 0,
    X = 1 << 0,
    Y = 1 << 1,
    Both = X | Y
}