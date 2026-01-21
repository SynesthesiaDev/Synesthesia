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
    private BackgroundContainer2d _outline = null!;
    private BackgroundContainer2d _backgroundContainer2d = null!;
    private BarebonesTextbox _textbox = null!;

    public string Hint { get; set; } = string.Empty;

    public readonly Bindable<bool> Focused = new(false);
    public readonly Bindable<string> Text = new(string.Empty);

    protected override void OnLoading()
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
                                    _textbox = new BarebonesTextbox
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
        Focused.OnValueChange(_ => UpdateVisualState());
        UpdateVisualState();
        
        Text.BindTo(_textbox.Text);
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
        if (!Contains(e.MousePosition)) return;
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

        
        _outline.FadeBackgroundTo(borderColor, 150, Easing.OutCubic);
        
        _backgroundContainer2d.FadeBackgroundTo(IsHovered ? DefaultEngineColorCombination.Surface1.Hovered : DefaultEngineColorCombination.Surface1.Normal, 100, Easing.OutCubic);
    }

    public void OnFocusGained()
    {
        Focused.Value = true;
        _textbox.OnFocusGained();
        UpdateVisualState();
    }

    public void OnFocusLost()
    {
        Focused.Value = false;
        _textbox.OnFocusLost();
        UpdateVisualState();
    }

    public void OnCharacterTyped(char character)
    {
        _textbox.OnCharacterTyped(character);
    }

    protected override void Dispose(bool isDisposing)
    {
        Text.Dispose();
        Focused.Dispose();
        
        base.Dispose(isDisposing);
    }
}