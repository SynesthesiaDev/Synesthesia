// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Components.Two.Barebones;
using Synesthesia.Engine.Events;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Util;
using Synesthesia.Engine.Util.Bindables;

namespace Synesthesia.Engine.Components.Two.Default;

public class DefaultTextbox : CompositeDrawable2D, IAcceptsFocus
{
    private static readonly ComplexColor border_active_color = ComplexColor.Single(EngineBranding.PURPLE);
    private static readonly ComplexColor border_inactive_color = ComplexColor.Single(EngineBranding.SLATE0);

    public bool IsFocused { get; private set; }

    public readonly Bindable<bool> IsPassword = new Bindable<bool>(false);
    public int MaxLenght { get; init; }
    public Func<string, bool>? Filter { get; init; }

    public EventDispatcher<string> OnCommit => textbox.OnCommit;
    public Bindable<string> Text => textbox.Text;
    public Bindable<Color> SelectionColor => textbox.SelectionColor;
    public bool HasSelection => textbox.HasSelection;
    public string SelectedText => textbox.SelectedText;

    public Drawable2D OwningDrawable => this;


    private Box2D background = null!;
    private BarebonesTextbox textbox = null!;

    protected override void OnLoading()
    {
        Children =
        [
            background = new Box2D
            {
                RelativeSizeAxes = Axes.Both,
                Color = EngineBranding.BACKGROUND0,
                BorderColor = ComplexColor.Single(EngineBranding.SLATE0),
                BorderThickness = 2,
                CornerRadius = 10
            },
            new Container2D
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Margin = new Vector4(10),
                Children =
                [
                    textbox = new BarebonesTextbox
                    {
                        RelativeSizeAxes = Axes.Both,
                        TextColor = EngineBranding.TEXT1,
                        MaxLenght = MaxLenght,
                        Filter = Filter,
                        IsPassword = IsPassword.Value,
                        Caret = () => new BarebonesTextboxCaret
                        {
                            RelativeSizeAxes = Axes.Y,
                            Size = new Vector2(1, 0),
                        }
                    }
                ]
            },
        ];
    }

    protected override void LoadComplete()
    {
        IsPassword.OnValueChange(e => textbox.IsPassword = e.NewValue);
    }

    private void updateVisualState()
    {
        var backgroundColor = ColorUtil.FromComponentState(IsHovered, EngineBranding.BACKGROUND0, 0.2f);
        var borderColor = ColorUtil.FromComponentState(IsFocused, IsHovered, border_active_color, border_inactive_color, 0.2f);

        background.FadeColorTo(backgroundColor, 150, Easing.OutCubic);
        background.FadeBorderColorTo(borderColor, 150, Easing.OutCubic);
    }

    protected internal override bool OnMouseDown(ICursorInputEvent e)
    {
        InputHandler.FocusedDrawable = this;
        return true;
    }

    protected internal override bool OnHover(IPositionalInputEvent e)
    {
        IsHovered = true;
        updateVisualState();
        return true;
    }

    protected internal override void OnHoverLost(IPositionalInputEvent e)
    {
        IsHovered = false;
        updateVisualState();
    }

    public void OnFocusGained()
    {
        IsFocused = true;
        textbox.OnFocusGained();
        updateVisualState();
    }

    public void OnFocusLost()
    {
        IsFocused = false;
        textbox.OnFocusLost();
        updateVisualState();
    }

    public void OnTextTyped(string text) => textbox.OnTextTyped(text);


    protected override void Dispose(bool isDisposing)
    {
        IsPassword.Dispose();
        base.Dispose(isDisposing);
    }
}
