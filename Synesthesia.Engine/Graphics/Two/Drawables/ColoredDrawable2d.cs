
using Raylib_cs;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;

namespace Synesthesia.Engine.Graphics.Two.Drawables;

public abstract class ColoredDrawable2d : Drawable2d
{
    public Color Color { get; set; } = Color.White;


    public Animation<Color> FadeColorTo(Color newColor, long duration, Easing easing)
    {
        return TransformTo
        (
            nameof(Color),
            Color,
            newColor,
            duration,
            easing,
            Transforms.COLOR,
            (color) => { Color = color; }
        );
    }
}