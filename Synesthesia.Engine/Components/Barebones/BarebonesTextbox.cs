using Common.Bindable;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Components.Barebones;

public class BarebonesTextbox : CompositeDrawable2d, IAcceptsFocus
{
    public required Func<AbstractTextboxCaret> Caret { get; init; }

    public readonly Bindable<string> Text = new(string.Empty);

    private const long initial_repeat_delay = 500;
    private const long repeat_rate = 50;

    private bool backspaceHeld = false;
    private long backspacePressTime = -1L;

    public bool IsFocused { get; set; }

    public Drawable2d GetOwningDrawable() => this;

    public TextDrawable TextDrawable = null!;

    public AbstractTextboxCaret CaretDrawable = null!;

    protected override void OnLoading()
    {
        Children =
        [
            new FillFlowContainer2d
            {
                RelativeSizeAxes = Axes.Both,
                Spacing = 1,
                Children =
                [
                    TextDrawable = new TextDrawable { Text = string.Empty },
                    CaretDrawable = Caret.Invoke(),
                ]
            }
        ];

        CaretDrawable.Alpha = 0;
    }

    protected override void LoadComplete()
    {
        Text.OnValueChange(e => TextDrawable.Text = e.NewValue);

        Scheduler.Value.Repeating(repeat_rate, _ =>
        {
            var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (backspaceHeld && now - backspacePressTime >= initial_repeat_delay)
            {
                Text.Value = Text.Value.RemoveLastN(1);
            }
        });
    }

    public void OnCharacterTyped(char character)
    {
        Text.Value += character;
    }

    protected internal override bool OnKeyDown(KeyboardKey e)
    {
        switch (e)
        {
            case KeyboardKey.Backspace:
            {
                backspaceHeld = true;
                backspacePressTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                Text.Value = Text.Value.RemoveLastN(1);
                return true;
            }
            case KeyboardKey.Enter:
            {
                InputManager.FocusedDrawable = null;
                return true;
            }
        }

        return base.OnKeyDown(e);
    }

    protected internal override void OnKeyUp(KeyboardKey e)
    {
        if (e == KeyboardKey.Backspace) backspaceHeld = false;
        base.OnKeyUp(e);
    }

    public void OnFocusGained()
    {
        IsFocused = true;
        CaretDrawable.Show();
    }

    public void OnFocusLost()
    {
        IsFocused = false;
        CaretDrawable.Hide();
    }

    protected override void Dispose(bool isDisposing)
    {
        Text.Dispose();
        base.Dispose(isDisposing);
    }

    public abstract class AbstractTextboxCaret : CompositeDrawable2d
    {
        public abstract void Show();
        public abstract void Hide();
    }

    public class BarebonesTextboxCaret : AbstractTextboxCaret
    {
        public const long BLINKING_SPEED = 500; //half a second is the standard

        public Easing BlinkingEasing { get; set; } = Easing.OutCubic;

        private DrawableBox2d caretBox = null!;

        protected override void OnLoading()
        {
            Children =
            [
                new Container2d
                {
                    RelativeSizeAxes = Axes.Both,
                    Children =
                    [
                        caretBox = new DrawableBox2d { RelativeSizeAxes = Axes.Both }
                    ],
                },
            ];
        }

        protected override void LoadComplete()
        {
            var animationSequence = new AnimationSequence.Builder()
                .Add(caretBox.FadeFromTo(1f, 0f, BLINKING_SPEED, BlinkingEasing))
                .Add(caretBox.FadeFromTo(0f, 1f, BLINKING_SPEED, BlinkingEasing))
                .IsLooping(true)
                .Build();

            Animator.Value.AddAnimation(animationSequence);
        }

        public override void Show()
        {
            FadeTo(1f, 150, Easing.OutCubic);
        }

        public override void Hide()
        {
            FadeTo(0f, 150, Easing.OutCubic);
        }
    }
}
