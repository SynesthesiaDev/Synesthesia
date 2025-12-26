using Raylib_cs;

namespace Synesthesia.Engine.Graphics.Three;

public abstract class ColoredDrawable3d : Drawable3d
{
    public Color Color { get; set; } = Color.White;
}