// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using Synesthesia.Engine.Input;

namespace Synesthesia.VisualTests.Tests;

public class ScrollableContainerTest : VisualTest
{
    private FillFlowContainer2d contentFillFlow = null!;
    private ScrollableContainer scrollableContainer = null!;

    protected override void OnLoading()
    {
        Children =
        [
            scrollableContainer = new ScrollableContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(400, 200),
                ScrollContent =
                [
                    contentFillFlow = new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Vertical,
                        Children =
                        [

                        ]
                    }
                ]
            }
        ];

        for (int i = 1; i < 100; i++)
        {
            contentFillFlow.AddChild(new TextDrawable
            {
                Text = $"Random Text {i}",
                Color = Color.White,
                FontSize = 24
            });
        }

        AddAssert("scroll position is 0", () => scrollableContainer.ScrollPosition == 0.0);

        AddStep("Scroll using mouse", () =>
        {
            InputManager.EnqueueEvent(new MouseMoveInputEvent(scrollableContainer.GetScreenSpaceCenter()));
            InputManager.EnqueueEvent(new MouseWheelInputEvent(-1));
        });

        AddAssert("scroll position is 80", () => Equals(scrollableContainer.ScrollPosition, 80.0));

        AddStep("Scroll down to bottom", () => scrollableContainer.ScrollTo(float.MaxValue));

        AddAssert("scroll position is maximum", () => Equals(scrollableContainer.ScrollPosition, scrollableContainer.MaxScrollPosition));

        AddStep("Remove half of children", () =>
        {
            for (int i = 0; i < 50; i++)
            {
                var child = contentFillFlow.Children.ToList().RandomFixed();
                contentFillFlow.RemoveChild(child);
            }
        });

        AddAssert("scroll position is maximum", () => Equals(scrollableContainer.ScrollPosition, scrollableContainer.MaxScrollPosition));

        AddStep("Reset Scroll", () => scrollableContainer.ResetScrollPosition());

        AddAssert("scroll position is 0", () => scrollableContainer.ScrollPosition == 0.0);

        base.OnLoading();
    }
}
