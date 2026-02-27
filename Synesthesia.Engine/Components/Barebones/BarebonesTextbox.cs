using System.Numerics;
using Common.Bindable;
using Common.Event;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Font;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Utility;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Components.Barebones;

public class BarebonesTextbox : CompositeDrawable2d, IAcceptsFocus
{
    public required Func<AbstractTextboxCaret> Caret { get; init; }

    private static readonly Color selection_color = Color.Blue;
    private const float selection_alpha = 0.5f;

    public readonly Bindable<string> Text = new(string.Empty);

    public readonly EventDispatcher<string> OnCommit = Pooled.STRING_DISPATCHER_POOL.Rent();

    private const long initial_repeat_delay = 500;
    private const long repeat_rate = 50;

    private int caretPosition;

    private int selectionStart;

    public bool HasSelection => selectionStart != caretPosition;

    public string SelectedText => HasSelection ? Text.Value[selectionLow..selectionHigh] : string.Empty;

    private int selectionLow => Math.Min(selectionStart, caretPosition);

    private int selectionHigh => Math.Max(selectionStart, caretPosition);

    private KeyboardKey? heldKey;

    private long heldKeyPressTime = -1L;

    public bool IsFocused { get; set; }

    public Drawable2d GetOwningDrawable() => this;

    public Text2d Text2d = null!;

    public AbstractTextboxCaret CaretDrawable = null!;

    private Box2d selectionBox = null!;

    private ScrollableContainer scrollableContainer = null!;

    protected override void OnLoading()
    {
        Children =
        [
            scrollableContainer = new ScrollableContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollDirection = Direction.Horizontal,
                ScrollContent =
                [
                    selectionBox = new Box2d
                    {
                        RelativeSizeAxes = Axes.Y,
                        Color = selection_color,
                        Alpha = 0,
                        Width = 0,
                        Position = new Vector2(0, 0)
                    },
                    Text2d = new Text2d { Text = string.Empty },
                    CaretDrawable = Caret.Invoke()
                ]
            },
        ];

        CaretDrawable.Alpha = 0;
    }

    protected override void LoadComplete()
    {
        Text.OnValueChange(e =>
        {
            Text2d.Text = e.NewValue;
            updateVisualState();
        });

        Scheduler.Value.Repeating(repeat_rate, _ =>
        {
            var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (heldKey.HasValue && now - heldKeyPressTime >= initial_repeat_delay)
            {
                var shift = KeyboardKey.LeftShift.IsDown() || KeyboardKey.RightShift.IsDown();
                handleNavigationKey(heldKey.Value, shift);
            }
        });
    }


    private void moveCaret(int newPosition, bool extendSelection)
    {
        newPosition = Math.Clamp(newPosition, 0, Text.Value.Length);
        if (!extendSelection) selectionStart = newPosition;

        caretPosition = newPosition;
        updateVisualState();
    }

    private void setCaret(int position)
    {
        position = Math.Clamp(position, 0, Text.Value.Length);
        caretPosition = position;
        selectionStart = position;
        updateVisualState();
    }

    private void insertAtCaret(string text)
    {
        if (HasSelection) deleteSelection();

        var pos = caretPosition;
        Text.Value = Text.Value[..caretPosition] + text;
        setCaret(pos + text.Length);
        updateVisualState();
    }

    private void deleteSelection()
    {
        var startPos = selectionLow;
        var endPos = selectionHigh;
        setCaret(startPos);
        Text.Value = Text.Value[..startPos] + Text.Value[endPos..];
    }

    private void deleteForward()
    {
        if (HasSelection)
        {
            deleteSelection();
            return;
        }

        if (caretPosition == Text.Value.Length) return;
        Text.Value = Text.Value[..caretPosition] + Text.Value[(caretPosition + 1)..];
    }

    private void deleteBackwards()
    {
        if (HasSelection)
        {
            deleteSelection();
            return;
        }

        if (caretPosition == 0) return;
        var pos = caretPosition - 1;
        setCaret(pos);
        Text.Value = Text.Value[..pos] + Text.Value[(pos + 1)..];
    }

    private void handleNavigationKey(KeyboardKey key, bool shift)
    {
        switch (key)
        {
            case KeyboardKey.Left:
            {
                if (HasSelection && !shift)
                {
                    setCaret(selectionLow);
                }
                else
                {
                    moveCaret(caretPosition - 1, shift);
                }

                break;
            }
            case KeyboardKey.Right:
            {
                if (HasSelection && !shift)
                {
                    setCaret(selectionHigh);
                }
                else
                {
                    moveCaret(caretPosition + 1, shift);
                }

                break;
            }
            case KeyboardKey.Backspace:
            {
                deleteBackwards();
                break;
            }
            case KeyboardKey.Delete:
            {
                deleteForward();
                break;
            }
        }

        updateVisualState();
    }

    public void OnCharacterTyped(char character)
    {
        insertAtCaret(character.ToString());
    }

    private void updateVisualState()
    {
        if (Text2d.Font is null) return;

        float fontSize = Text2d.FontSize;
        float spacing = Text2d.Spacing;
        var font = Text2d.Font;
        var text = Text.Value;

        float caretX = measurePartialText(font, text, caretPosition, fontSize, spacing);
        var newCaretPos = CaretDrawable.Position with { X = caretX };
        CaretDrawable.MoveTo(newCaretPos, 50, Easing.OutCirc);

        scrollToCaret(caretX);

        if (HasSelection && !text.IsEmpty())
        {
            float selLowX = measurePartialText(font, text, selectionLow, fontSize, spacing);
            float selHighX = measurePartialText(font, text, selectionHigh, fontSize, spacing);

            var newSelectionPos = selectionBox.Position with { X = selLowX };
            selectionBox.MoveTo(newSelectionPos, 100, Easing.OutCirc);

            selectionBox.ResizeWidthTo(selHighX - selLowX, 100, Easing.OutCirc);
            selectionBox.FadeTo(selection_alpha, 100, Easing.OutCirc);
        }
        else
        {
            selectionBox.Alpha = 0f;

        }
    }

    private void scrollToCaret(float caretX)
    {
        var viewportWidth = scrollableContainer.Width;

        var currentScroll = scrollableContainer.ScrollPosition;
        var padding = 20f;

        if (caretX > currentScroll + viewportWidth - padding)
        {
            scrollableContainer.ScrollTo(caretX - viewportWidth + padding, 100);
        }
        else if (caretX < currentScroll + padding)
        {
            scrollableContainer.ScrollTo(Math.Max(0, caretX - padding), 100);
        }
    }

    private static float measurePartialText(FontHandle font, string text, int charCount, float fontSize, float spacing)
    {
        if (charCount <= 0) return 0f;
        var partial = text[..charCount];
        return Raylib.MeasureTextEx(font.NativeFont, partial, fontSize, spacing).X;
    }

    protected internal override bool OnKeyDown(KeyboardKey e)
    {
        var shift = KeyboardKey.LeftShift.IsDown() || KeyboardKey.RightShift.IsDown();
        var ctrl = KeyboardKey.LeftControl.IsDown() || KeyboardKey.RightControl.IsDown();

        switch (e)
        {
            case KeyboardKey.Left:
            case KeyboardKey.Right:
            case KeyboardKey.Backspace:
            case KeyboardKey.Delete:
            {
                heldKey = e;
                heldKeyPressTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                handleNavigationKey(e, shift);
                return true;
            }

            case KeyboardKey.A when ctrl:
            {
                selectionBox.Width = 0;
                selectionStart = 0;
                caretPosition = Text.Value.Length;
                updateVisualState();
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
        if (heldKey == e) heldKey = null;
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
        selectionStart = caretPosition;
        updateVisualState();
        OnCommit.Dispatch(Text.Value);
    }

    protected override void Dispose(bool isDisposing)
    {
        Text.Dispose();
        if(OnCommit.IsPooled) Pooled.STRING_DISPATCHER_POOL.Return(OnCommit);
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

        public Easing BlinkingEasingIn { get; set; } = Easing.InCubic;
        public Easing BlinkingEasingOut { get; set; } = Easing.OutCubic;

        private Box2d caretBox = null!;

        protected override void OnLoading()
        {
            Children =
            [
                new Container2d
                {
                    RelativeSizeAxes = Axes.Both,
                    Children =
                    [
                        caretBox = new Box2d { RelativeSizeAxes = Axes.Both }
                    ],
                },
            ];
        }

        protected override void LoadComplete()
        {
            var animationSequence = new AnimationSequence.Builder()
                .Add(caretBox.FadeFromTo(1f, 0f, BLINKING_SPEED, BlinkingEasingIn))
                .Add(caretBox.FadeFromTo(0f, 1f, BLINKING_SPEED, BlinkingEasingOut))
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
