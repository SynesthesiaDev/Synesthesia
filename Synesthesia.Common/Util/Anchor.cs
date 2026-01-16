namespace Common.Util;

[Flags]
public enum Anchor
{
    None = 0,
    Top = 1 << 0,
    CentreVertical = 1 << 1,
    Bottom = 1 << 2,
    Left = 1 << 3,
    CentreHorizontal = 1 << 4,
    Right = 1 << 5,

    TopLeft = Top | Left,
    TopCentre = Top | CentreHorizontal,
    TopRight = Top | Right,

    CentreLeft = CentreVertical | Left,
    Centre = CentreVertical | CentreHorizontal,
    CentreRight = CentreVertical | Right,

    BottomLeft = Bottom | Left,
    BottomCentre = Bottom | CentreHorizontal,
    BottomRight = Bottom | Right
}