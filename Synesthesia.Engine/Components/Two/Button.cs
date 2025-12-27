using System.Numerics;
using Common.Util;
using Synesthesia.Engine.Animation;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.Engine.Components.Two;

public class Button : CompositeDrawable2d
{
    public string Text
    {
        get => _textDrawable.Text;
        set => _textDrawable.Text = value;
    }

    private BackgroundContainer2d _backgroundContainer;
    private TextDrawable _textDrawable;

    public Button()
    {
        Size = new Vector2(120, 40);
        Children =
        [
            _backgroundContainer = new BackgroundContainer2d
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColor = Defaults.Background2,
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

    protected internal override bool OnMouseDown(MouseEvent e)
    {
        ScaleTo(0.9f, 2000, Easing.OutQuint);
        return true;
    }

    protected internal override void OnMouseUp(MouseEvent e)
    {
        ScaleTo(1f, 1000, Easing.OutElastic);
    }

    protected internal override bool OnHover(HoverEvent e)
    {
        _backgroundContainer.BackgroundColor = Defaults.Background3;
        return true;
    }

    protected internal override void OnHoverLost(HoverEvent e)
    {
        _backgroundContainer.BackgroundColor = Defaults.Background2;
    }
}