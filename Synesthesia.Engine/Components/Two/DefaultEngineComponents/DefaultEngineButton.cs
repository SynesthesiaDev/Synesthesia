using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public class DefaultEngineButton : DisableableContainer
{
    public string Text
    {
        get => textDrawable.Text;
        set => textDrawable.Text = value;
    }

    public float FontSize
    {
        get => textDrawable.FontSize;
        set => textDrawable.FontSize = value;
    }

    public Color TextColor
    {
        get => textDrawable.Color;
        set => textDrawable.Color = value;
    }

    public DefaultEngineColorCombination ColorCombination { get; init; } = DefaultEngineColorCombination.SURFACE2;

    public Action? OnClick { get; set; }

    private BackgroundContainer2d backgroundContainer;
    private TextDrawable textDrawable;

    protected override void OnLoading()
    {
        updateVisualState();
        base.OnLoading();
    }

    private void updateVisualState()
    {
        if (IsHovered)
        {
            backgroundContainer.FadeBackgroundTo(ColorCombination.Hovered, 100, Easing.InCubic);
        }
        else
        {
            backgroundContainer.FadeBackgroundTo(ColorCombination.Normal, 100, Easing.OutCubic);
        }
    }

    public DefaultEngineButton()
    {
        Children =
        [
            backgroundContainer = new BackgroundContainer2d
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColor = ColorCombination.Normal,
                BackgroundCornerRadius = 5,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    textDrawable = new TextDrawable
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = string.Empty
                    }
                ]
            }
        ];
    }

    protected internal override bool OnMouseDown(PointInput e)
    {
        if (Disabled) return false;
        backgroundContainer.ScaleTo(0.9f, 2000, Easing.OutQuint);
        return true;
    }

    protected internal override void OnMouseUp(PointInput e)
    {
        if (Disabled) return;
        backgroundContainer.ScaleTo(1f, 1000, Easing.OutElastic);
        if (Contains(e.MousePosition)) OnClick?.Invoke();
    }

    protected internal override bool OnHover(HoverEvent e)
    {
        IsHovered = true;
        updateVisualState();
        return base.OnHover(e);
    }

    protected internal override void OnHoverLost(HoverEvent e)
    {
        IsHovered = false;
        updateVisualState();
        base.OnHoverLost(e);
    }
}