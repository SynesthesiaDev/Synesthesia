using System.Numerics;
using Common.Bindable;
using Common.Util;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public class DefaultEngineCheckbox : CompositeDrawable2d, IDisablable
{
    public readonly Bindable<bool> Checked = new(true);

    private bool _disabled = false;

    public bool Disabled
    {
        get => _disabled;
        set
        {
            if (_disabled != value)
            {
                var newColor = value
                    ? _textDrawable.Color.ChangeBrightness(-0.5f)
                    : _textDrawable.Color.ChangeBrightness(0.5f);
                _textDrawable.Color = newColor;
            }

            _disabled = value;
            _checkbox.Disabled = value;
        }
    }

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

    private TextDrawable _textDrawable;
    private Checkbox _checkbox;

    public DefaultEngineColorCombination ColorCombination { get; init; } = DefaultEngineColorCombination.Surface2;

    public DefaultEngineCheckbox()
    {
        Children =
        [
            new BackgroundContainer2d
            {
                RelativeSizeAxes = Axes.Both,
                Children =
                [
                    _checkbox = new Checkbox
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                    },
                    _textDrawable = new TextDrawable
                    {
                        Text = string.Empty,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                ]
            }
        ];
        _checkbox.Checked.BindTo(Checked);
    }

    protected internal override void OnUpdate()
    {
        base.OnUpdate();
        _checkbox.Size = new Vector2(Size.Y);
    }

    protected internal override bool OnHover(HoverEvent e)
    {
        _checkbox.Hovered.Value = true;
        return true;
    }

    protected internal override void OnHoverLost(HoverEvent e)
    {
        _checkbox.Hovered.Value = false;
        base.OnHoverLost(e);
    }

    protected internal override void OnMouseUp(MouseEvent e)
    {
        if (Disabled) return;
        Checked.Value = !Checked.Value;

        base.OnMouseUp(e);
    }

    protected internal override bool OnMouseDown(MouseEvent e)
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
        private static DefaultEngineColorCombination _checkboxColor = DefaultEngineColorCombination.Accent;
        private static DefaultEngineColorCombination _colors = DefaultEngineColorCombination.Surface1;

        private readonly BindablePool _pool = new();
        public readonly Bindable<bool> Checked;
        public readonly Bindable<bool> Hovered;

        public Checkbox()
        {
            Checked = _pool.Borrow(true);
            Hovered = _pool.Borrow(true);
        }
        
        private Vector2 InnerSize => new(Size.X - 10);

        private DrawableBox2d _box = null!;
        private DrawableBox2d _backgroundBox = null!;

        protected internal override void OnUpdate()
        {
            base.OnUpdate();
            _box.Size = InnerSize;
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
                        _backgroundBox = new DrawableBox2d
                        {
                            RelativeSizeAxes = Axes.Both,
                            Color = Defaults.Background1,
                            CornerRadius = 5,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },

                        _box = new DrawableBox2d
                        {
                            Size = InnerSize,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Color = Defaults.Accent,
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
                    _box.ScaleTo(1f, 150, Easing.OutBack);
                }
                else
                {
                    _box.ScaleTo(0f, 100, Easing.InBack);
                }
            });

            Hovered.OnValueChange(e =>
            {
                _box.FadeColorTo(e.NewValue ? _checkboxColor.Hovered : _checkboxColor.Normal, 100, Easing.In);
                _backgroundBox.FadeColorTo(e.NewValue ? _colors.Hovered : _colors.Normal, 100, Easing.In);
            });

            base.OnLoading();
        }
        
        protected override void Dispose(bool isDisposing)
        {
            _pool.Dispose();
            base.Dispose(isDisposing);
        }
    }
}