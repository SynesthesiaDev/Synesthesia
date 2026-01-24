using System.Numerics;
using Common.Bindable;
using Common.Util;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Components.Barebones;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public class DefaultEngineTextbox : DisableableContainer, IAcceptsFocus
{
    private BackgroundContainer2d outline = null!;
    private BackgroundContainer2d backgroundContainer2d = null!;
    private BarebonesTextbox textbox = null!;

    public string Hint { get; set; } = string.Empty;

    public readonly Bindable<bool> Focused = new(false);
    public readonly Bindable<string> Text = new(string.Empty);

    protected override void OnLoading()
    {
        Children =
        [
            outline = new BackgroundContainer2d
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColor = Defaults.BACKGROUND3,
                BackgroundCornerRadius = 10,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    backgroundContainer2d = new BackgroundContainer2d
                    {
                        RelativeSizeAxes = Axes.Both,
                        BackgroundColor = Defaults.BACKGROUND1,
                        BackgroundCornerRadius = 10,
                        Margin = new Vector4(1),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children =
                        [
                            new BackgroundContainer2d
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Margin = new Vector4(5),
                                Children =
                                [
                                    textbox = new BarebonesTextbox
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Caret = () => new BarebonesTextbox.BarebonesTextboxCaret
                                        {
                                            RelativeSizeAxes = Axes.Y,
                                            Size = new Vector2(1, 0)
                                        }
                                    }
                                ]
                            },
                        ],
                    }
                ]
            }
        ];
        
        base.OnLoading();
    }

    protected override void LoadComplete()
    {
        Focused.OnValueChange(_ => updateVisualState());
        updateVisualState();
        
        Text.BindTo(textbox.Text);
    }

    protected internal override bool OnHover(HoverEvent e)
    {
        IsHovered = true;
        updateVisualState();
        return true;
    }

    protected internal override void OnHoverLost(HoverEvent e)
    {
        IsHovered = false;
        updateVisualState();
    }

    protected internal override void OnMouseUp(PointInput e)
    {
        if (!Contains(e.MousePosition)) return;
        InputManager.FocusedDrawable = this;
    }

    protected internal override bool OnMouseDown(PointInput e)
    {
        return true;
    }

    public Drawable2d GetOwningDrawable() => this;

    private void updateVisualState()
    {
        var borderColor = Focused.Value switch
        {
            true when IsHovered => DefaultEngineColorCombination.ACCENT.Hovered,
            true => DefaultEngineColorCombination.ACCENT.Normal,
            _ => IsHovered ? DefaultEngineColorCombination.SURFACE3.Hovered : DefaultEngineColorCombination.SURFACE3.Normal
        };

        
        outline.FadeBackgroundTo(borderColor, 150, Easing.OutCubic);
        
        backgroundContainer2d.FadeBackgroundTo(IsHovered ? DefaultEngineColorCombination.SURFACE1.Hovered : DefaultEngineColorCombination.SURFACE1.Normal, 100, Easing.OutCubic);
    }

    public void OnFocusGained()
    {
        Focused.Value = true;
        textbox.OnFocusGained();
        updateVisualState();
    }

    public void OnFocusLost()
    {
        Focused.Value = false;
        textbox.OnFocusLost();
        updateVisualState();
    }

    public void OnCharacterTyped(char character)
    {
        textbox.OnCharacterTyped(character);
    }

    protected override void Dispose(bool isDisposing)
    {
        Text.Dispose();
        Focused.Dispose();
        
        base.Dispose(isDisposing);
    }
}