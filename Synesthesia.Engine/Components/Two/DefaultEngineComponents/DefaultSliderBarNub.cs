// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Util;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Components.Barebones;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public class DefaultSliderBarNub(BarebonesSliderBar owningSliderBar) : SliderBarNub(owningSliderBar)
{
    private DrawableBox2d nub = null!;

    protected override void OnLoading()
    {
        RelativeSizeAxes = Axes.Y;
        Anchor = Anchor.CentreLeft;
        Origin = Anchor.CentreLeft;
        Width = 10;
        Children =
        [
            nub = new DrawableBox2d
            {
                RelativeSizeAxes = Axes.Both,
                Color = Defaults.BACKGROUND3,
                CornerRadius = 5,
            }
        ];
    }

    protected internal override bool OnHover(HoverEvent e)
    {
        nub.FadeColorTo(Defaults.BACKGROUND4, 100, Easing.InCubic);
        return true;
    }

    protected internal override void OnHoverLost(HoverEvent e)
    {
        nub.FadeColorTo(Defaults.BACKGROUND3, 100, Easing.OutCubic);
    }
}
