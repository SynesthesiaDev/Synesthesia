// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Util;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Components.Barebones;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public class DefaultSliderBarBody : SliderBarBody
{
    private DrawableBox2d filledBox = null!;
    private BackgroundContainer2d container = null!;

    protected override void OnLoading()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;

        Children =
        [
            container = new BackgroundContainer2d
            {
                Height = 10,
                RelativeSizeAxes = Axes.X,
                BackgroundColor = Defaults.BACKGROUND2,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    filledBox = new DrawableBox2d
                    {
                        RelativeSizeAxes = Axes.Y,
                        Color = Defaults.ACCENT,
                    }
                ]
            }
        ];

        base.OnLoading();
    }

    protected internal override bool OnHover(HoverEvent e)
    {
        filledBox.FadeColorTo(Defaults.ACCENT.ChangeBrightness(0.4f), 100, Easing.InCubic);
        container.FadeBackgroundTo(Defaults.BACKGROUND2.ChangeBrightness(0.06f), 100, Easing.OutCubic);

        return true;
    }

    protected internal override void OnHoverLost(HoverEvent e)
    {
        filledBox.FadeColorTo(Defaults.ACCENT, 100, Easing.OutCubic);
        container.FadeBackgroundTo(Defaults.BACKGROUND2, 100, Easing.OutCubic);
    }

    public override void ValueChanged(float newValue)
    {
        filledBox.ResizeWidthTo(getWidth(newValue), 10, Easing.InCubic);
    }

    private float getWidth(float progress)
    {
        return MathUtil.ValueOf(progress, Size.X);
    }
}
