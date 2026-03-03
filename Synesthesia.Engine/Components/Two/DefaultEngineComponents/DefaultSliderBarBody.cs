// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Util;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Components.Barebones;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public class DefaultSliderBarBody(BarebonesSliderBar owningSliderBar) : SliderBarBody(owningSliderBar)
{
    private Box2d filledBox = null!;
    private Box2d backgroundBox = null!;
    private Container2d container = null!;

    private float? afterLoadProgress;

    protected override void OnLoading()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;

        Children =
        [
            container = new Container2d
            {
                Height = 10,
                RelativeSizeAxes = Axes.X,
                Masking = true,
                CornerRadius = 10,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    backgroundBox = new Box2d
                    {
                        RelativeSizeAxes = Axes.Both,
                        Color = Defaults.BACKGROUND2,
                    },
                    filledBox = new Box2d
                    {
                        RelativeSizeAxes = Axes.Y,
                        Color = Defaults.ACCENT,
                    }
                ]
            }
        ];

        base.OnLoading();
    }

    protected internal override bool OnHover(MouseMoveInputEvent e)
    {
        filledBox.FadeColorTo(Defaults.ACCENT.ChangeBrightness(0.4f), 100, Easing.InCubic);
        backgroundBox.FadeColorTo(Defaults.BACKGROUND2.ChangeBrightness(0.06f), 100, Easing.OutCubic);

        return true;
    }

    protected internal override void OnHoverLost(MouseMoveInputEvent e)
    {
        filledBox.FadeColorTo(Defaults.ACCENT, 100, Easing.OutCubic);
        backgroundBox.FadeColorTo(Defaults.BACKGROUND2, 100, Easing.OutCubic);
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if (Width != 0 && afterLoadProgress != null)
        {
            var p = afterLoadProgress.Value;
            afterLoadProgress = null;
            ValueChanged(p);
        }

        base.OnUpdate(frameInfo);
    }

    public override void ValueChanged(float newValue)
    {
        if (Width == 0)
        {
            afterLoadProgress = newValue;
            return;
        }

        filledBox.ResizeWidthTo(getWidth(newValue), 10, Easing.OutQuart);
    }

    private float getWidth(float progress)
    {
        return MathUtil.ValueOf(progress, Size.X);
    }
}
