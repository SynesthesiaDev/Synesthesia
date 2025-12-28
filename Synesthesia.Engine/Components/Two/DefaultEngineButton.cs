using System.Numerics;
using Common.Util;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.Engine.Components.Two;

public class DefaultEngineButton : DisableableContainer
{
    public string Text
    {
        get => _textDrawable.Text;
        set => _textDrawable.Text = value;
    }

    public Action? OnClick { get; set; } = null;

    private BackgroundContainer2d _backgroundContainer;
    private TextDrawable _textDrawable;

    public DefaultEngineButton()
    {
        Size = new Vector2(120, 40);
        Children =
        [
            _backgroundContainer = new BackgroundContainer2d
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColor = Defaults.Background2,
                BackgroundCornerRadius = 5,
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

    protected internal override bool OnMouseDown(MouseEvent e)
    {
        ScaleTo(0.9f, 2000, Easing.OutQuint);
        return true;
    }

    protected internal override void OnMouseUp(MouseEvent e)
    {
        ScaleTo(1f, 1000, Easing.OutElastic);
        if (!Disabled) OnClick?.Invoke();
    }

    protected internal override bool OnHover(HoverEvent e)
    {
        _backgroundContainer.FadeBackgroundTo(Defaults.Background3, 100, Easing.InCubic);
        return true;
    }

    protected internal override void OnHoverLost(HoverEvent e)
    {
        _backgroundContainer.FadeBackgroundTo(Defaults.Background2, 100, Easing.OutCubic);
    }
}