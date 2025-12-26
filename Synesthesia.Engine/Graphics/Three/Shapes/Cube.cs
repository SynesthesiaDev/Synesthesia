using Raylib_cs;

namespace Synesthesia.Engine.Graphics.Three.Shapes;

public class Cube : ColoredDrawable3d
{
    protected override void OnDraw3d()
    {
        Raylib.DrawCube(Position, Size.X, Size.Y, Size.Z, Color);
    }
}