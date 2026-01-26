using System.Numerics;
using Raylib_cs;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

public class DrawableBox2d : ColoredDrawable2d
{
    public float CornerRadius { get; set; } = 0f;

    protected override void OnDraw2d()
    {
        if (CornerRadius <= 0)
        {
            Raylib.DrawRectangleV(Vector2.Zero, Size, ApplyAlpha(Color));
        }
        else
        {
            var shortestSide = Math.Min(Size.X, Size.Y);
            var roundness = CornerRadius * 2 / shortestSide;

            roundness = Math.Clamp(roundness, 0f, 1f);

            var rect = new Rectangle(0, 0, Size.X, Size.Y);

            Raylib.DrawRectangleRounded(rect, roundness, 8, ApplyAlpha(Color));
        }
    }
}