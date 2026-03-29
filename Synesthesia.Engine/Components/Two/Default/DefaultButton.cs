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

public class DefaultButton : Container2d
{
    public string Text
    {
        get => text2d.Text;
        set => text2d.Text = value;
    }

    public float FontSize
    {
        get => text2d.FontSize;
        set => text2d.FontSize = value;
    }

    public Color TextColor
    {
        get => text2d.Color;
        set => text2d.Color = value;
    }

    public bool Disabled = false;

    public Action? OnClick { get; set; }

    private Box2d background;
    private Container2d contianer;
    private Text2d text2d;

    private Color styleBackground = EngineBranding.PURPLE;
    private Color styleForeground = EngineBranding.TEXT2;
    private ComplexColor styleBorderColor = ComplexColor.GradientVertical(Color.White, Color.Transparent);

    public Style ButtonStyle
    {
        get;
        set
        {
            if(field == value) return;
            field = value;
            updateVisualState();
        }

    } = Style.Secondary;

    protected override void OnLoading()
    {
        updateVisualState();
        base.OnLoading();
    }

    private void updateVisualState()
    {
        switch (ButtonStyle)
        {
            case Style.Primary:
                styleBackground = EngineBranding.PURPLE;
                styleForeground = EngineBranding.TEXT2;
                styleBorderColor = ComplexColor.GradientVertical(EngineBranding.PINK, Color.Transparent);
                break;
            case Style.Secondary:
                styleBackground = EngineBranding.PINK;
                styleForeground = Color.Black;
                styleBorderColor = ComplexColor.GradientVertical(Color.White.WithOpacity(0.7f), Color.Transparent);
                break;
            case Style.Tertiary:
                styleBackground = Color.Transparent;
                styleForeground = EngineBranding.TEXT2;
                styleBorderColor = ComplexColor.GradientVertical(EngineBranding.PINK, EngineBranding.PURPLE);
                background.BorderThickness = 3;
                break;
        }

        background.BorderColor = styleBorderColor;
        text2d.FadeColorTo(styleForeground, 100, Easing.OutCubic);
        background.FadeColorTo(styleForeground, 100, Easing.OutCubic);

        if (IsHovered)
        {
            background.FadeColorTo(styleBackground.Lighten(0.1f), 100, Easing.InCubic);
        }
        else
        {
            background.FadeColorTo(styleBackground, 100, Easing.OutCubic);
        }
    }

    public DefaultButton()
    {
        Children =
        [
            contianer = new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    background = new Box2d
                    {
                        RelativeSizeAxes = Axes.Both,
                        Color = styleBackground,
                        CornerRadius = 999,
                        BorderThickness = 2
                    },
                    text2d = new Text2d
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = string.Empty,
                        Color = styleForeground,
                        FontSize = 24
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

