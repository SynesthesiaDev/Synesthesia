// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;

namespace Synesthesia.Engine.Components.Two.Barebones;

public class BarebonesTextboxCaret : TextboxCaret
{
    public const long BLINKING_SPEED = 500; //half a second is the standard

    public Easing BlinkingEasingIn { get; set; } = Easing.InCubic;
    public Easing BlinkingEasingOut { get; set; } = Easing.OutCubic;

    private Box2D caretBox = null!;

    protected override void OnLoading()
    {
        Children =
        [
            new Container2D
            {
                RelativeSizeAxes = Axes.Both,
                Children =
                [
                    caretBox = new Box2D { RelativeSizeAxes = Axes.Both }
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
