using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.VisualTests.Tests;

public class ShowcaseTest : VisualTest
{
    private Container2d stage = null!;

    private Container2d appFrame = null!;
    private Container2d sidebar = null!;
    private Container2d content = null!;

    private FillFlowContainer2d autoStack = null!;
    private Container2d stackItemGrowing = null!;

    private Container2d insetParent = null!;
    private Container2d insetChild = null!;

    private DrawableBox2d anchorMarker = null!;

    private const float spacing = 12f;

    private static bool near(float a, float b, float eps = 1.0f) => MathF.Abs(a - b) <= eps;

    protected override void OnLoading()
    {
        Children =
        [
            stage = new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    // Big background
                    new DrawableBox2d
                    {
                        RelativeSizeAxes = Axes.Both,
                        Color = new Color(18, 18, 18, 255),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    },

                    // "App frame" we can resize for the demo
                    appFrame = new Container2d
                    {
                        Size = new Vector2(980, 560),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children =
                        [
                            new DrawableBox2d
                            {
                                RelativeSizeAxes = Axes.Both,
                                Color = new Color(28, 28, 28, 255),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre
                            },

                            // App layout: sidebar + content area
                            new FillFlowContainer2d
                            {
                                RelativeSizeAxes = Axes.Both,
                                Direction = Direction.Horizontal,
                                Spacing = spacing,
                                Children =
                                [
                                    sidebar = new Container2d
                                    {
                                        Width = 240,
                                        RelativeSizeAxes = Axes.Y,
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Children =
                                        [
                                            new DrawableBox2d
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Color = new Color(45, 45, 45, 255),
                                                Anchor = Anchor.TopLeft,
                                                Origin = Anchor.TopLeft
                                            },

                                            // AutoSize stack inside the sidebar
                                            autoStack = new FillFlowContainer2d
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Direction = Direction.Vertical,
                                                Spacing = 8f,
                                                Anchor = Anchor.TopLeft,
                                                Origin = Anchor.TopLeft,
                                                Margin = new Vector4(12, 12, 12, 12),
                                                Children =
                                                [
                                                    makeStackItem(216, 34, new Color(90, 150, 240, 255)),
                                                    stackItemGrowing = makeStackItem(216, 46, new Color(240, 190, 80, 255)),
                                                    makeStackItem(216, 34, new Color(120, 220, 140, 255)),
                                                ]
                                            }
                                        ]
                                    },

                                    content = new Container2d
                                    {
                                        FillRemainingAxes = Axes.Both,
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Children =
                                        [
                                            new DrawableBox2d
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Color = new Color(32, 32, 32, 255),
                                                Anchor = Anchor.TopLeft,
                                                Origin = Anchor.TopLeft
                                            },

                                            // Inset demo: RelativeSize + Margin
                                            insetParent = new Container2d
                                            {
                                                Size = new Vector2(520, 260),
                                                Anchor = Anchor.TopLeft,
                                                Origin = Anchor.TopLeft,
                                                Margin = new Vector4(18, 18, 0, 0),
                                                Children =
                                                [
                                                    new DrawableBox2d
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Color = new Color(40, 40, 40, 255),
                                                        Anchor = Anchor.TopLeft,
                                                        Origin = Anchor.TopLeft
                                                    },

                                                    insetChild = new Container2d
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Margin = new Vector4(24, 18, 36, 42), // L,T,R,B
                                                        Anchor = Anchor.TopLeft,
                                                        Origin = Anchor.TopLeft,
                                                        Children =
                                                        [
                                                            new DrawableBox2d
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                Color = new Color(80, 140, 220, 110),
                                                                Anchor = Anchor.TopLeft,
                                                                Origin = Anchor.TopLeft
                                                            }
                                                        ]
                                                    }
                                                ]
                                            },

                                            // Anchor/Origin marker demo inside a fixed "cell"
                                            new Container2d
                                            {
                                                Size = new Vector2(320, 260),
                                                Anchor = Anchor.TopRight,
                                                Origin = Anchor.TopRight,
                                                Margin = new Vector4(0, 18, 18, 0),
                                                Children =
                                                [
                                                    new DrawableBox2d
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Color = new Color(40, 40, 40, 255),
                                                        Anchor = Anchor.TopLeft,
                                                        Origin = Anchor.TopLeft
                                                    },
                                                    new DrawableBox2d
                                                    {
                                                        Size = new Vector2(8, 8),
                                                        Color = new Color(160, 160, 160, 255),
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre
                                                    },
                                                    anchorMarker = new DrawableBox2d
                                                    {
                                                        Size = new Vector2(18, 18),
                                                        Color = Color.Gold,
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre
                                                    }
                                                ]
                                            }
                                        ]
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }
        ];

        AddStep("Reset baseline", () =>
        {
            appFrame.Size = new Vector2(980, 560);
            sidebar.Width = 240;

            stackItemGrowing.Size = new Vector2(216, 46);

            insetParent.Size = new Vector2(520, 260);
            insetChild.Margin = new Vector4(24, 18, 36, 42);

            anchorMarker.Anchor = Anchor.Centre;
            anchorMarker.Origin = Anchor.Centre;
            anchorMarker.Position = Vector2.Zero;
        }, runNextImmediately: true);

        AddAssert("Inset child size matches parent - margins", () =>
        {
            var m = insetChild.Margin;
            var expectedW = MathF.Max(0f, insetParent.Width - m.X - m.Z);
            var expectedH = MathF.Max(0f, insetParent.Height - m.Y - m.W);
            return near(insetChild.Width, expectedW, 1.2f) && near(insetChild.Height, expectedH, 1.2f);
        });

        AddStep("Animate growing stack item height", () =>
        {
            stackItemGrowing.ResizeHeightTo(110f, 650, Easing.OutCubic);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until stack item grew", () => near(stackItemGrowing.Height, 110f, 1.5f), timeout: 3000);

        AddStep("Toggle sidebar width (show fill remainder)", () =>
        {
            var target = sidebar.Width < 260 ? 340f : 220f;
            sidebar.ResizeWidthTo(target, 650, Easing.OutCubic);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until sidebar width updated", () => sidebar.Width is > 200f and < 380f, timeout: 3000);

        AddStep("Resize app frame smaller", () =>
        {
            appFrame.ResizeTo(new Vector2(820, 460), 700, Easing.OutCubic);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until app frame resized", () => near(appFrame.Width, 820f, 2.0f) && near(appFrame.Height, 460f, 2.0f), timeout: 4000);

        AddStep("Move anchor marker to BottomRight", () =>
        {
            anchorMarker.Anchor = Anchor.BottomRight;
            anchorMarker.Origin = Anchor.Centre;
        });

        AddStep("Move anchor marker to TopLeft", () =>
        {
            anchorMarker.Anchor = Anchor.TopLeft;
            anchorMarker.Origin = Anchor.Centre;
        });

        AddStep("Move anchor marker to Centre (Origin=BottomRight)", () =>
        {
            anchorMarker.Anchor = Anchor.Centre;
            anchorMarker.Origin = Anchor.BottomRight;
        });

        base.OnLoading();
    }

    private static Container2d makeStackItem(float w, float h, Color c)
    {
        return new Container2d
        {
            Size = new Vector2(w, h),
            Children =
            [
                new DrawableBox2d
                {
                    RelativeSizeAxes = Axes.Both,
                    Color = c,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            ]
        };
    }
}
