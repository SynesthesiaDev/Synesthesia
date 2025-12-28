
using Raylib_cs;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;

namespace Synesthesia.Engine.Graphics.Two.Drawables;

public abstract class ColoredDrawable2d : Drawable2d
{
    public Color Color { get; set; } = Color.White;

    public Animation<float> FadeAlphaTo(float newAlpha, long duration, Easing easing)
    {
        return TransformTo
        (
            nameof(Alpha),
            Alpha,
            newAlpha,
            duration,
            easing,
            Transforms.Float,
            alpha => { Alpha = alpha; }
        );
    }

    public Animation<Color> FadeColorTo(Color newColor, long duration, Easing easing)
    {
        return TransformTo
        (
            nameof(Color),
            Color,
            newColor,
            duration,
            easing,
            Transforms.Color,
            (color) => { Color = color; }
        );
    }
}