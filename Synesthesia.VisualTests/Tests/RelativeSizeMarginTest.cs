// Synesthesia.VisualTests/Tests/RelativeSizeMarginTest.cs

using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.VisualTests.Tests;

public class RelativeSizeMarginTest : VisualTest
{
    private Container2d parent = null!;
    private Container2d child = null!;
    private Container2d childBr = null!;

    private static bool near(float a, float b, float eps = 0.9f) => MathF.Abs(a - b) <= eps;

    protected override void OnLoading()
    {
        Scale = new Vector2(0.5f);
        Children =
        [
            new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    parent = new Container2d
                    {
                        Size = new Vector2(640, 360),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children =
                        [
                            new DrawableBox2d
                            {
                                RelativeSizeAxes = Axes.Both,
                                Color = new Color(30, 30, 30, 255),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre
                            },

                            child = new Container2d
                            {
                                RelativeSizeAxes = Axes.Both,
                                Margin = new Vector4(24, 18, 36, 42), // L, T, R, B
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Children =
                                [
                                    new DrawableBox2d
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Color = new Color(80, 140, 220, 255),
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft
                                    }
                                ]
                            },

                            // Bottom-right anchored variant (catches "wrong side margin sign" regressions visually)
                            childBr = new Container2d
                            {
                                RelativeSizeAxes = Axes.Both,
                                Margin = new Vector4(10, 30, 70, 14), // asymmetric on purpose
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight,
                                Children =
                                [
                                    new DrawableBox2d
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Color = new Color(220, 120, 90, 140), // translucent overlay
                                        Anchor = Anchor.BottomRight,
                                        Origin = Anchor.BottomRight
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }
        ];

        AddStep("Reset parent + children", () =>
        {
            parent.Size = new Vector2(640, 360);

            child.RelativeSizeAxes = Axes.Both;
            child.Anchor = Anchor.TopLeft;
            child.Origin = Anchor.TopLeft;
            child.Margin = new Vector4(24, 18, 36, 42);

            childBr.RelativeSizeAxes = Axes.Both;
            childBr.Anchor = Anchor.BottomRight;
            childBr.Origin = Anchor.BottomRight;
            childBr.Margin = new Vector4(10, 30, 70, 14);
        }, runNextImmediately: true);

        AddWaitUntil("Wait for layout", expectedMatchesCurrent, timeout: 1500);

        AddAssert("Both children size = parent - margins", expectedMatchesCurrent,
            "Anchor/Origin should not affect the computed size, only positioning. Margins should subtract L+R and T+B.");

        AddStep("Animate parent resize bigger", () =>
        {
            parent.ResizeTo(new Vector2(820, 420), 450, Easing.OutCubic);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until parent resized", () =>
            near(parent.Width, 820f, 1.0f) && near(parent.Height, 420f, 1.0f), timeout: 2500);

        AddWaitUntil("Wait until children updated", expectedMatchesCurrent, timeout: 2000);

        AddAssert("Both children still match after resize", expectedMatchesCurrent);

        AddStep("Animate parent resize smaller", () =>
        {
            parent.ResizeTo(new Vector2(520, 240), 450, Easing.OutCubic);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until parent resized (small)", () =>
            near(parent.Width, 520f, 1.0f) && near(parent.Height, 240f, 1.0f), timeout: 2500);

        AddWaitUntil("Wait until children updated (small)", expectedMatchesCurrent, timeout: 2000);

        AddAssert("Both children match after shrink", expectedMatchesCurrent);

        base.OnLoading();
    }

    private bool expectedMatchesCurrent()
    {
        return expectedMatches(child) && expectedMatches(childBr);
    }

    private bool expectedMatches(Container2d c)
    {
        var m = c.Margin;

        var expectedW = parent.Width - m.X - m.Z;
        var expectedH = parent.Height - m.Y - m.W;

        // If margins exceed parent size, expected can go negative; clamp expectation at 0
        expectedW = MathF.Max(0f, expectedW);
        expectedH = MathF.Max(0f, expectedH);

        return near(c.Width, expectedW) && near(c.Height, expectedH);
    }
}
