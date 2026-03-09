using System.Numerics;
using Common.Bindable;
using Common.Event;
using Common.Util;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Components.Barebones;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public class DefaultTextbox : DisableableContainer, IAcceptsFocus
{
    private Container2d mainContainer = null!;
    private Box2d background = null!;
    private BarebonesTextbox textbox = null!;

    public string Hint { get; set; } = string.Empty;

    public readonly Bindable<bool> Focused = new(false);
    public readonly Bindable<string> Text = new(string.Empty);

    public BarebonesTextbox UnderlyingTextBox => textbox;

    public EventDispatcher<string> OnCommit => UnderlyingTextBox.OnCommit;

    protected override void OnLoading()
    {
        Children =
        [
            mainContainer = new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Margin = new Vector4(1),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                BorderThickness = 1,
                BorderColor = ComplexColor.Single(Defaults.BACKGROUND3),
                CornerRadius = 10,
                Children =
                [
                    background = new Box2d
                    {
                        RelativeSizeAxes = Axes.Both,
                        Color = Defaults.BACKGROUND1,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },

                    new Container2d
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
            },
        ];

        base.OnLoading();
    }

    protected override void LoadComplete()
    {
        Focused.OnValueChange(_ => updateVisualState());
        updateVisualState();

        Text.BindTo(textbox.Text);
    }

    protected internal override bool OnHover(MouseMoveInputEvent e)
    {
        IsHovered = true;
        updateVisualState();
        return true;
    }

    protected internal override void OnHoverLost(MouseMoveInputEvent e)
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


        // mainContainer.FadeBackgroundTo(borderColor, 150, Easing.OutCubic);
        mainContainer.FadeBorderColorTo(ComplexColor.Single(borderColor), 150, Easing.OutCubic);

        background.FadeColorTo(IsHovered ? DefaultEngineColorCombination.SURFACE1.Hovered : DefaultEngineColorCombination.SURFACE1.Normal, 100, Easing.OutCubic);
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
