using System.Numerics;
using Common.Bindable;
using Common.Util;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public class DefaultEngineCheckbox : CompositeDrawable2d, IDisablable
{
    public readonly Bindable<bool> Checked = new(true);

    private bool disabled = false;

    public bool Disabled
    {
        get => disabled;
        set
        {
            if (disabled != value)
            {
                var newColor = value
                    ? textDrawable.Color.ChangeBrightness(-0.5f)
                    : textDrawable.Color.ChangeBrightness(0.5f);
                textDrawable.Color = newColor;
            }

            disabled = value;
            checkbox.Disabled = value;
        }
    }

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

    private TextDrawable textDrawable;
    private Checkbox checkbox;

    public DefaultEngineColorCombination ColorCombination { get; init; } = DefaultEngineColorCombination.SURFACE2;

    public DefaultEngineCheckbox()
    {
        Children =
        [
            new BackgroundContainer2d
            {
                RelativeSizeAxes = Axes.Both,
                Children =
                [
                    checkbox = new Checkbox
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                    },
                    textDrawable = new TextDrawable
                    {
                        Text = string.Empty,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                ]
            }
        ];
        checkbox.Checked.BindTo(Checked);
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        base.OnUpdate(frameInfo);
        checkbox.Size = new Vector2(Size.Y);
    }

    protected internal override bool OnHover(HoverEvent e)
    {
        checkbox.Hovered.Value = true;
        return true;
    }

    protected internal override void OnHoverLost(HoverEvent e)
    {
        checkbox.Hovered.Value = false;
        base.OnHoverLost(e);
    }

    protected internal override void OnMouseUp(PointInput e)
    {
        if (Disabled) return;
        Checked.Value = !Checked.Value;

        base.OnMouseUp(e);
    }

    protected internal override bool OnMouseDown(PointInput e)
    {
        return !Disabled;
    }

    protected override void Dispose(bool isDisposing)
    {
        Checked.Dispose();
        base.Dispose(isDisposing);
    }

    private class Checkbox : DisableableContainer
    {
        private static DefaultEngineColorCombination checkboxColor = DefaultEngineColorCombination.ACCENT;
        private static DefaultEngineColorCombination colors = DefaultEngineColorCombination.SURFACE1;

        private readonly BindablePool pool = new();
        public readonly Bindable<bool> Checked;
        public readonly Bindable<bool> Hovered;

        public Checkbox()
        {
            Checked = pool.Borrow(true);
            Hovered = pool.Borrow(true);
        }

        private Vector2 innerSize => new(Size.X - 10);

        private DrawableBox2d box = null!;
        private DrawableBox2d backgroundBox = null!;

        protected internal override void OnUpdate(FrameInfo frameInfo)
        {
            base.OnUpdate(frameInfo);
            box.Size = innerSize;
        }

        protected override void OnLoading()
        {
            Children =
            [
                new BackgroundContainer2d
                {
                    RelativeSizeAxes = Axes.Both,
                    BackgroundCornerRadius = 5,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children =
                    [
                        backgroundBox = new DrawableBox2d
                        {
                            RelativeSizeAxes = Axes.Both,
                            Color = Defaults.BACKGROUND1,
                            CornerRadius = 5,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },

                        box = new DrawableBox2d
                        {
                            Size = innerSize,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Color = Defaults.ACCENT,
                            Visible = Checked.Value,
                            CornerRadius = 3,
                        }
                    ]
                }
            ];

            Checked.OnValueChange(e =>
            {
                if (e.NewValue)
                {
                    box.ScaleTo(1f, 150, Easing.OutBack);
                }
                else
                {
                    box.ScaleTo(0f, 100, Easing.InBack);
                }
            });

            Hovered.OnValueChange(e =>
            {
                box.FadeColorTo(e.NewValue ? checkboxColor.Hovered : checkboxColor.Normal, 100, Easing.In);
                backgroundBox.FadeColorTo(e.NewValue ? colors.Hovered : colors.Normal, 100, Easing.In);
            });

            base.OnLoading();
        }

        protected override void Dispose(bool isDisposing)
        {
            pool.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
