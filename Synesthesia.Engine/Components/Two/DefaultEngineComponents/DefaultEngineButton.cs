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
        get => _textDrawable.Text;
        set => _textDrawable.Text = value;
    }

    public float FontSize
    {
        get => _textDrawable.FontSize;
        set => _textDrawable.FontSize = value;
    }

    public Color TextColor
    {
        get => _textDrawable.Color;
        set => _textDrawable.Color = value;
    }

    public DefaultEngineColorCombination ColorCombination { get; init; } = DefaultEngineColorCombination.Surface2;

    public Action? OnClick { get; set; }

    private BackgroundContainer2d _backgroundContainer;
    private TextDrawable _textDrawable;

    protected override void OnLoading()
    {
        updateVisualState();
        base.OnLoading();
    }

    private void updateVisualState()
    {
        if (IsHovered)
        {
            _backgroundContainer.FadeBackgroundTo(ColorCombination.Hovered, 100, Easing.InCubic);
        }
        else
        {
            _backgroundContainer.FadeBackgroundTo(ColorCombination.Normal, 100, Easing.OutCubic);
        }
    }

    public DefaultEngineButton()
    {
        Children =
        [
            _backgroundContainer = new BackgroundContainer2d
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColor = ColorCombination.Normal,
                BackgroundCornerRadius = 5,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    _textDrawable = new TextDrawable
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
        _backgroundContainer.ScaleTo(0.9f, 2000, Easing.OutQuint);
        return true;
    }

    protected internal override void OnMouseUp(PointInput e)
    {
        if (Disabled) return;
        _backgroundContainer.ScaleTo(1f, 1000, Easing.OutElastic);
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