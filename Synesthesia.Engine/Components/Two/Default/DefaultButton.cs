// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Graphics.Two.Text;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Util;

namespace Synesthesia.Engine.Components.Two.Default;

public class DefaultButton : Container2D
{
    private const long transform_length = 150;

    public string Text
    {
        get => text2D.Text;
        set => text2D.Text = value;
    }

    public float FontSize
    {
        get => text2D.FontSize;
        set => text2D.FontSize = value;
    }

    public Color TextColor
    {
        get => text2D.Color;
        set => text2D.Color = value;
    }

    public bool Disabled = false;

    public Action? OnClick { get; set; }

    private Box2D background;
    private Container2D contianer;
    private Text2D text2D;

    private Color styleBackground = EngineBranding.PURPLE;
    private Color styleForeground = EngineBranding.TEXT2;
    private ComplexColor styleBorderColor = ComplexColor.GradientVertical(Color.White, Color.Transparent);

    public Style ButtonStyle
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            updateStyle();
        }
    } = Style.Secondary;

    protected override void OnLoading()
    {
        updateStyle();
        base.OnLoading();
    }

    private void updateStyle()
    {
        if (Disabled)
        {
            styleForeground = EngineBranding.TEXT0;
            if (ButtonStyle == Style.Tertiary)
            {
                background.BorderThickness = 3;
                styleBackground = Color.Transparent;
                styleBorderColor = ComplexColor.Single(EngineBranding.SLATE1);
            }
            else
            {
                styleBackground = EngineBranding.SLATE1;
                background.BorderThickness = 0;
            }
        }
        else
        {
            switch (ButtonStyle)
            {
                case Style.Primary:
                    styleBackground = EngineBranding.PURPLE;
                    styleForeground = EngineBranding.TEXT2;
                    background.BorderThickness = 0;
                    break;
                case Style.Secondary:
                    styleBackground = EngineBranding.PINK;
                    styleForeground = Color.Black;
                    background.BorderThickness = 0;
                    break;
                case Style.Tertiary:
                    styleBackground = Color.Transparent;
                    styleForeground = EngineBranding.TEXT2;
                    styleBorderColor = ComplexColor.GradientVertical(EngineBranding.PINK, EngineBranding.PURPLE);
                    background.BorderThickness = 3;
                    break;
            }
        }

        background.FadeBorderColorTo(styleBorderColor, transform_length, Easing.OutCubic);
        text2D.FadeColorTo(styleForeground, transform_length, Easing.OutCubic);
        background.FadeColorTo(styleForeground, transform_length, Easing.OutCubic);
        updateVisualState();
    }

    private void updateVisualState()
    {
        if (IsHovered)
        {
            background.FadeColorTo(styleBackground.Lighten(0.2f), transform_length, Easing.OutCubic);
            if (ButtonStyle == Style.Tertiary) background.FadeBorderColorTo(styleBorderColor.Lighten(0.25f), transform_length, Easing.OutCubic);
        }
        else
        {
            background.FadeColorTo(styleBackground, transform_length, Easing.InCubic);
            if (ButtonStyle == Style.Tertiary) background.FadeBorderColorTo(styleBorderColor, transform_length, Easing.InCubic);
        }
    }

    public DefaultButton()
    {
        Children =
        [
            contianer = new Container2D
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    background = new Box2D
                    {
                        RelativeSizeAxes = Axes.Both,
                        Color = styleBackground,
                        CornerRadius = 999,
                        BorderThickness = 3
                    },
                    text2D = new Text2D
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = string.Empty,
                        Color = styleForeground,
                        FontSize = 20
                    }
                ]
            }
        ];
    }

    protected internal override bool OnMouseDown(ICursorInputEvent e)
    {
        if (Disabled) return false;
        contianer.ScaleTo(0.9f, 2000, Easing.OutQuint);
        return true;
    }

    protected internal override void OnMouseUp(ICursorInputEvent e)
    {
        if (Disabled) return;
        contianer.ScaleTo(1f, 1000, Easing.OutElastic);
        if (Contains(InputHandler.MousePosition)) OnClick?.Invoke();
    }

    protected internal override bool OnHover(IPositionalInputEvent e)
    {
        IsHovered = true;
        updateVisualState();
        return base.OnHover(e);
    }

    protected internal override void OnHoverLost(IPositionalInputEvent e)
    {
        IsHovered = false;
        updateVisualState();
        base.OnHoverLost(e);
    }

    public enum Style
    {
        Primary,
        Secondary,
        Tertiary
    }
}
