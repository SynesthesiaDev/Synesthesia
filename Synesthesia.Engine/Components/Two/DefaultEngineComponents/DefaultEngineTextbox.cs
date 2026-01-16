using System.Numerics;
using Common.Bindable;
using Common.Util;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public class DefaultEngineTextbox : DisableableContainer, IAcceptsFocus
{
    private BackgroundContainer2d _outline;
    private BackgroundContainer2d _backgroundContainer2d;

    public readonly Bindable<bool> Focused = new(false);

    public DefaultEngineTextbox()
    {
        Children =
        [
            _outline = new BackgroundContainer2d
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColor = Defaults.Background3,
                BackgroundCornerRadius = 10,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    _backgroundContainer2d = new BackgroundContainer2d
                    {
                        RelativeSizeAxes = Axes.Both,
                        BackgroundColor = Defaults.Background1,
                        BackgroundCornerRadius = 10,
                        Margin = new Vector4(1),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                ]
            }
        ];

        Focused.OnValueChange(e => { UpdateVisualState(); });
    }

    protected internal override bool OnHover(HoverEvent e)
    {
        IsHovered = true;
        UpdateVisualState();
        return true;
    }

    protected internal override void OnHoverLost(HoverEvent e)
    {
        IsHovered = false;
        UpdateVisualState();
    }

    protected internal override void OnMouseUp(PointInput e)
    {
        if(!Contains(e.MousePosition)) return;
        InputManager.FocusedDrawable = this;
    }

    protected internal override bool OnMouseDown(PointInput e)
    {
        return true;
    }

    public Drawable2d GetOwningDrawable() => this;

    private void UpdateVisualState()
    {
        var borderColor = Focused.Value switch
        {
            true when IsHovered => DefaultEngineColorCombination.Accent.Hovered,
            true => DefaultEngineColorCombination.Accent.Normal,
            _ => IsHovered ? DefaultEngineColorCombination.Surface3.Hovered : DefaultEngineColorCombination.Surface3.Normal
        };

        _outline.FadeBackgroundTo(borderColor, 100, Easing.OutCubic);
        _backgroundContainer2d.FadeBackgroundTo(IsHovered ? DefaultEngineColorCombination.Surface1.Hovered : DefaultEngineColorCombination.Surface1.Normal, 100, Easing.OutCubic);
    }

    public void OnFocusGained()
    {
        Focused.Value = true;
        UpdateVisualState();
    }

    public void OnFocusLost()
    {
        Focused.Value = false;
        UpdateVisualState();
    }
}