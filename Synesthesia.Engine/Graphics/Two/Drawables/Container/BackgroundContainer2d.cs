using Raylib_cs;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Container;

public class BackgroundContainer2d : Container2d
{
    private static readonly Color empty_color = new(0, 0, 0, 0);

    public Color BackgroundColor { get; set; } = empty_color;

    public float BackgroundAlpha { get; set; } = 1f;

    public float BackgroundCornerRadius { get; set; } = 0f;

    private readonly DrawableBox2d background = new();

    protected override void OnDraw2d()
    {
        DrawBackground();
        base.OnDraw2d();
    }

    protected override void OnLoading()
    {
        background.Load();
        base.OnLoading();
    }

    protected void DrawBackground()
    {
        if(!background.IsLoaded) return;
        if (BackgroundColor.A == 0) return;
        background.Size = Size;
        background.Parent = this;

        background.CornerRadius = BackgroundCornerRadius;
        background.Color = BackgroundColor;
        background.Alpha = BackgroundAlpha;

        background.OnDraw();
    }

    public Animation<float> FadeBackgroundAlphaTo(float newAlpha, long duration, Easing easing)
    {
        return TransformTo
        (
            nameof(BackgroundAlpha),
            Alpha,
            newAlpha,
            duration,
            easing,
            Transforms.FLOAT,
            alpha => { BackgroundAlpha = alpha; }
        );
    }

    public Animation<float> TransformBackgroundCornerRadiusTo(float newRadius, long duration, Easing easing)
    {
        return TransformTo
        (
            nameof(BackgroundCornerRadius),
            BackgroundCornerRadius,
            newRadius,
            duration,
            easing,
            Transforms.FLOAT,
            radius => { BackgroundCornerRadius = radius; }
        );
    }

    public Animation<Color> FadeBackgroundTo(Color newColor, long duration, Easing easing)
    {
        return TransformTo
        (
            nameof(BackgroundColor),
            BackgroundColor,
            newColor,
            duration,
            easing,
            Transforms.COLOR,
            color => { BackgroundColor = color; }
        );
    }

    protected override void Dispose(bool isDisposing)
    {
        background.Dispose();
        base.Dispose(isDisposing);
    }
}