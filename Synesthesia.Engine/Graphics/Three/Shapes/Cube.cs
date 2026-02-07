using System.Numerics;
using Raylib_cs;

namespace Synesthesia.Engine.Graphics.Three.Shapes;

public class Cube : ColoredDrawable3d
{
    protected override void OnDraw3d()
    {
        Raylib.DrawCube(Vector3.Zero, 1, 1, 1, ApplyAlpha(Color));
    }
}
