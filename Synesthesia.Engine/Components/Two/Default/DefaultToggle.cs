// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Components.Two.Barebones;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Util;

namespace Synesthesia.Engine.Components.Two.Default;

public class DefaultToggle : BarebonesToggle
{
    private Box2D background = null!;
    private Box2D head = null!;

    protected override void OnLoading()
    {
        Children =
        [
            new Container2D
            {
                RelativeSizeAxes = Axes.Both,
                Children =
                [
                    background = new Box2D
                    {
                        RelativeSizeAxes = Axes.Both,
                        Color = EngineBranding.BACKGROUND0,
                        BorderThickness = 2,
                        CornerRadius = 999,
                        BorderColor = ComplexColor.GradientHorizontal(EngineBranding.PURPLE, EngineBranding.PINK),
                    },
                    head = new Box2D
                    {
                        Size = new Vector2(24, 15),
                        Color = EngineBranding.PINK,
                        CornerRadius = 999,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        X = getHeadPosition(Checked.Value)
                    }
                ]
            }
        ];
    }

    protected internal override bool OnHover(IPositionalInputEvent e)
    {
        IsHovered = true;
        updateState();
        return true;
    }

    protected internal override void OnHoverLost(IPositionalInputEvent e)
    {
        IsHovered = false;
        updateState();
    }

    private void updateState()
    {
        var isChecked = Checked.Value;
        head.MoveXTo(getHeadPosition(isChecked), 150, Easing.OutBack);

        var headColor = isChecked ? EngineBranding.PINK : EngineBranding.SLATE1;
        var borderColor = isChecked ? ComplexColor.GradientHorizontal(EngineBranding.PURPLE, EngineBranding.PINK) : ComplexColor.Single(EngineBranding.SLATE1);

        var headHover = IsHovered ? headColor.Lighten(0.2f) : headColor;
        var borderHover = IsHovered ? borderColor.Lighten(0.2f) : borderColor;

        head.FadeColorTo(headHover, 150, Easing.OutCubic);
        background.FadeBorderColorTo(borderHover, 150, Easing.OutCubic);
    }

    private float getHeadPosition(bool state) => state ? 37f : 5f;

    protected override void OnToggle(bool toggled)
    {
        updateState();
    }
}
