
using Raylib_cs;

namespace Synesthesia.Engine.Graphics.Two.Drawables;

public abstract class ColoredDrawable2d : Drawable2d
{
    public Color Color { get; set; } = Color.White;
}