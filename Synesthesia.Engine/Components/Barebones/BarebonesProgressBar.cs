// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Bindable;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Components.Barebones;

public class BarebonesProgressBar : CompositeDrawable2d
{
    public Color BackgroundColor
    {
        get => backgroundContainer.BackgroundColor;
        set => backgroundContainer.BackgroundColor = value;
    }

    public float BackgroundAlpha
    {
        get => backgroundContainer.BackgroundAlpha;
        set => backgroundContainer.BackgroundAlpha = value;
    }

    public float CornerRadius
    {
        get => backgroundContainer.BackgroundCornerRadius;
        set
        {
            box.CornerRadius = value;
            backgroundContainer.BackgroundCornerRadius = value;
        }
    }

    public Easing AnimationEasing { get; set; } = Easing.In;

    public int AnimationTime { get; set; } = 100;

    public readonly Bindable<float> Progress = new(0.0f);

    private BackgroundContainer2d backgroundContainer = null!;
    private DrawableBox2d box = null!;

    private float lastProgressValue = 0.0f;

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        base.OnUpdate(frameInfo);

        if(Progress.Value.Equals(lastProgressValue)) return;

        box.ResizeWidthTo(getWidth(Progress.Value), AnimationTime, AnimationEasing);
        lastProgressValue = Progress.Value;
    }

    protected override void OnLoading()
    {
        Children =
        [
            backgroundContainer = new BackgroundContainer2d
            {
                RelativeSizeAxes = Axes.Both,
                Children =
                [
                    box = new DrawableBox2d
                    {
                        RelativeSizeAxes = Axes.Y,
                        Color = Color.Red,
                    }
                ]
            }
        ];

        backgroundContainer.BackgroundColor = Color.DarkGray;
        box.Color = Defaults.ACCENT;

        base.OnLoading();
    }

    private float getWidth(float progress)
    {
        return MathUtil.ValueOf(progress, Size.X);
    }
}
