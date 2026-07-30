// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Events;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Graphics.Two.Text;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Platform.Host;
using Synesthesia.Engine.Util;
using Synesthesia.Engine.Util.Bindables;
using Synesthesia.Engine.Util.Pooling;
using Synesthesia.Utils.Extensions;

namespace Synesthesia.Engine.Components.Two.Barebones;

public class BarebonesTextbox : CompositeDrawable2D, IAcceptsFocus
{
    [Singleton]
    private IClipboard clipboard = null!;

    public Drawable2D OwningDrawable => this;

    public required Func<TextboxCaret> Caret { get; init; }

    public readonly Bindable<Color> SelectionColor = new(EngineBranding.PINK.WithOpacity(0.5f));

    public readonly Bindable<string> Text = new(string.Empty);

    public readonly EventDispatcher<string> OnCommit = Pooled.STRING_DISPATCHER_POOL.Rent();

    private int caretPosition;

    private int selectionStart;

    public bool HasSelection => selectionStart != caretPosition;

    public string SelectedText => HasSelection ? Text.Value[selectionLow..selectionHigh] : string.Empty;

    private int selectionLow => Math.Min(selectionStart, caretPosition);

    private int selectionHigh => Math.Max(selectionStart, caretPosition);

    public int MaxLenght { get; init; }

    public Func<string, bool>? Filter { get; init; }

    public bool IsPassword
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Text.TriggerChange();
            updateVisualState();
        }
    } = false;

    public bool IsFocused { get; private set; }

    public Text2D Text2D = null!;

    public TextboxCaret CaretDrawable = null!;

    private Box2D selectionBox = null!;

    private ScrollableContainer scrollableContainer = null!;

    public Color TextColor
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (Text2D != null) Text2D.Color = field;
        }
    } = Color.White;

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
                    selectionBox = new Box2D
                    {
                        RelativeSizeAxes = Axes.Y,
                        Color = SelectionColor.Value,
                        Width = 0,
                        Alpha = 0,
                        Position = new Vector2(0, 0)
                    },
                    Text2D = new Text2D
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = string.Empty,
                        Color = TextColor
                    },
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
            Text2D.Text = IsPassword ? getMaskedText(e.NewValue) : e.NewValue;
            updateVisualState();
        });

        SelectionColor.OnValueChange(e =>
        {
            selectionBox.FadeColorTo(e.NewValue, 150, Easing.OutCubic);
        });
    }

    private string getMaskedText(string text)
    {
        var len = text.Length;
        var masked = "";
        for (int i = 0; i < len; i++)
        {
            masked += "*";
        }

        return masked;
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

        text = text.Replace("\r", "").Replace("\n", "");

        if (Filter != null && !Filter(text)) return;

        if (MaxLenght > 0 && Text.Value.Length + text.Length > MaxLenght)
        {
            var allowed = MaxLenght - Text.Value.Length;
            if (allowed <= 0) return;
            text = text[..allowed];
        }

        caretPosition = Math.Clamp(caretPosition, 0, Text.Value.Length);
        var newText = Text.Value.Insert(caretPosition, text);

        caretPosition += text.Length;
        selectionStart = caretPosition;

        Text.Value = newText;
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

        if (caretPosition <= 0) return;

        int targetIndex = caretPosition - 1;
        string newText = Text.Value.Remove(targetIndex, 1);

        caretPosition = targetIndex;
        selectionStart = targetIndex;

        Text.Value = newText;
    }

    private void handleNavigationKey(Key key, bool shift, bool ctrl)
    {
        switch (key)
        {
            case Key.Left:
            {
                if (HasSelection && !shift && !ctrl)
                {
                    setCaret(selectionLow);
                }
                else
                {
                    var target = ctrl ? getNextWordPosition(-1) : caretPosition - 1;
                    moveCaret(target, shift);
                }

                break;
            }
            case Key.Right:
            {
                if (HasSelection && !shift && !ctrl)
                {
                    setCaret(selectionHigh);
                }
                else
                {
                    var target = ctrl ? getNextWordPosition(+1) : caretPosition + 1;
                    moveCaret(target, shift);
                }

                break;
            }
            case Key.Back:
            {
                if (ctrl && !HasSelection)
                {
                    selectionStart = getNextWordPosition(-1);
                }

                deleteBackwards();
                break;
            }
            case Key.Delete:
            {
                if (ctrl && !HasSelection)
                {
                    selectionStart = getNextWordPosition(1);
                }

                deleteForward();
                break;
            }
        }

        updateVisualState();
    }

    public void OnTextTyped(string text)
    {
        if (!IsFocused) return;
        insertAtCaret(text);
    }

    private void updateVisualState()
    {
        if (Text2D.Font is null) return;

        float fontSize = Text2D.FontSize;
        var font = Text2D.Font;
        var text = Text2D.Text;

        float caretX = measurePartialText(font, text, caretPosition, fontSize);
        var newCaretPos = CaretDrawable.Position with { X = caretX };
        CaretDrawable.MoveTo(newCaretPos, 50, Easing.OutCirc);

        scrollToCaret(caretX);

        if (HasSelection && !text.IsEmpty())
        {
            float selLowX = measurePartialText(font, text, selectionLow, fontSize);
            float selHighX = measurePartialText(font, text, selectionHigh, fontSize);

            var newSelectionPos = selectionBox.Position with { X = selLowX };

            selectionBox.MoveTo(newSelectionPos, 100, Easing.OutCirc);

            var targetWidth = selHighX - selLowX;
            if (selectionBox.Alpha <= 0.01f)
            {
                selectionBox.X = selLowX;
                selectionBox.Width = targetWidth;
            }

            selectionBox.ResizeWidthTo(targetWidth, 100, Easing.OutCirc);
            selectionBox.FadeTo(0.5f, 100, Easing.OutCirc);
        }
        else
        {
            selectionBox.FadeTo(0.0f, 100, Easing.OutCirc);
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

    private static float measurePartialText(Font font, string text, int charCount, float fontSize)
    {
        if (charCount <= 0 || text.IsEmpty()) return 0f;
        var partial = text[..charCount];
        return font.MeasureText(partial, fontSize).X;
    }

    protected internal override bool OnKeyDown(KeyboardInputEvent e)
    {
        var key = e.Key;
        if (!IsFocused) return false;

        var shift = Key.LShift.IsDown() || Key.RShift.IsDown();
        var ctrl = Key.LControl.IsDown() || Key.RControl.IsDown();

        switch (key)
        {
            case Key.Left:
            case Key.Right:
            case Key.Back:
            case Key.Delete:
            {
                handleNavigationKey(key, shift, ctrl);
                return true;
            }

            case Key.A when ctrl:
            {
                selectionBox.Width = 0;
                selectionStart = 0;
                caretPosition = Text.Value.Length;
                updateVisualState();
                return true;
            }

            case Key.Enter:
            {
                unfocusSelf();
                return true;
            }

            case Key.Escape:
            {
                if (HasSelection)
                {
                    selectionStart = caretPosition;
                    updateVisualState();
                }
                else
                {
                    unfocusSelf();
                }

                return true;
            }

            case Key.C when (ctrl && HasSelection):
            {
                clipboard.SetClipboardText(SelectedText);
                return true;
            }

            case Key.X when (ctrl && HasSelection):
            {
                clipboard.SetClipboardText(SelectedText);
                deleteSelection();
                return true;
            }

            case Key.V when ctrl:
            {
                var text = clipboard.GetClipboardText();
                if (text == null) return false;

                insertAtCaret(text);
                return true;
            }
        }

        return false;
    }

    private int getNextWordPosition(int direction)
    {
        string text = Text.Value;
        var pos = caretPosition;

        if (direction > 0)
        {
            while (pos < text.Length && char.IsWhiteSpace(text[pos])) pos++;
            while (pos < text.Length && !char.IsWhiteSpace(text[pos])) pos++;
        }
        else
        {
            pos--;
            while (pos > 0 && char.IsWhiteSpace(text[pos])) pos--;
            while (pos > 0 && !char.IsWhiteSpace(text[pos])) pos--;
            pos = Math.Max(0, pos);
        }

        return pos;
    }

    private void unfocusSelf() => InputHandler.FocusedDrawable = null;

    public void OnFocusGained()
    {
        IsFocused = true;
        CaretDrawable.Show();
        updateVisualState();
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
        if (OnCommit.IsPooled) Pooled.STRING_DISPATCHER_POOL.Return(OnCommit);
        base.Dispose(isDisposing);
    }
}
