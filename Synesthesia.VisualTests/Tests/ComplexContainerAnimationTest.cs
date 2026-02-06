using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.VisualTests.Tests;

public class ComplexContainerAnimationTest : VisualTest
{
    private ScrollableContainer scroll = null!;
    private FillFlowContainer2d flow = null!;

    private Container2d itemWrapper = null!;
    private DrawableBox2d itemBox = null!;

    // We’ll animate the *scroll container itself* too.
    private Container2d scrollHost = null!;

    private static bool near(float a, float b, float eps = 0.75f) => MathF.Abs(a - b) <= eps;

    protected override void OnLoading()
    {
        Scale = new Vector2(0.5f);
        Children =
        [
            scrollHost = new Container2d
            {
                Size = new Vector2(520, 320),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    scroll = new ScrollableContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        ScrollDirection = Direction.Vertical,
                        ScrollContent =
                        [
                            flow = new FillFlowContainer2d
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Direction = Direction.Vertical,
                                Spacing = 12f,
                                Children =
                                [
                                    makeItem(Color.DarkBlue),
                                    makeItem(Color.DarkPurple),
                                    makeAnimatedItem(),
                                    makeItem(Color.DarkGreen),
                                    makeItem(Color.DarkBrown),
                                    makeItem(Color.DarkGray),
                                ]
                            }
                        ]
                    }
                ]
            }
        ];

        AddStep("Reset (layout + animation baseline)", () =>
        {
            itemBox.Position = Vector2.Zero;
            itemBox.Scale = Vector2.One;
            itemBox.Alpha = 1f;

            scroll.ResetScrollPosition();

            scrollHost.Position = Vector2.Zero;
            scrollHost.Scale = Vector2.One;
            scrollHost.Alpha = 1f;
            scrollHost.Size = new Vector2(520, 320);
        }, runNextImmediately: true);

        AddAssert("Animated item starts at local (0,0)", () => near(itemBox.Position.X, 0f) && near(itemBox.Position.Y, 0f));
        AddAssert("Scroll starts at 0", () => Math.Abs(scroll.ScrollPosition - 0.0) <= 0.001);

        AddStep("Animate item box (inside FillFlow wrapper)", () =>
        {
            itemBox.MoveTo(new Vector2(140, 0), 450, Easing.OutCubic);
            itemBox.ScaleTo(1.25f, 450, Easing.OutBack);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until item box moved", () =>
            near(itemBox.Position.X, 140f) && near(itemBox.Position.Y, 0f), timeout: 1500);

        AddAssert("Item box reached x≈140", () => near(itemBox.Position.X, 140f));

        AddStep("Scroll down a bit", () =>
        {
            scroll.ScrollBy(160);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until scrolled (position changed)", () =>
            scroll.ScrollPosition >= 80.0, timeout: 2000);

        AddAssert("ScrollPosition increased", () => scroll.ScrollPosition > 0.0);

        AddStep("Animate container host (move + fade + scale)", () =>
        {
            scrollHost.MoveTo(new Vector2(-60, 30), 500, Easing.OutCubic);
            scrollHost.ScaleTo(0.92f, 500, Easing.OutCubic);
            scrollHost.FadeTo(0.65f, 500, Easing.OutQuad);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until host moved", () =>
            near(scrollHost.Position.X, -60f) && near(scrollHost.Position.Y, 30f), timeout: 2000);

        AddWaitUntil("Wait until host faded", () => scrollHost.Alpha <= 0.66f, timeout: 2000);

        AddAssert("Host alpha ~0.65", () => scrollHost.Alpha <= 0.66f);

        AddStep("Resize host container", () =>
        {
            scrollHost.ResizeTo(new Vector2(600, 260), 550, Easing.OutQuart);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until host resized", () =>
            near(scrollHost.Size.X, 600f) && near(scrollHost.Size.Y, 260f), timeout: 2500);

        AddAssert("Host width ~600", () => near(scrollHost.Size.X, 600f));

        AddStep("Restore host to normal", () =>
        {
            scrollHost.MoveTo(Vector2.Zero, 400, Easing.OutCubic);
            scrollHost.ScaleTo(1f, 400, Easing.OutCubic);
            scrollHost.FadeTo(1f, 300, Easing.OutQuad);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until restored", () =>
            near(scrollHost.Position.X, 0f) &&
            near(scrollHost.Position.Y, 0f) &&
            scrollHost.Alpha >= 0.99f &&
            near(scrollHost.Scale.X, 1f) &&
            near(scrollHost.Scale.Y, 1f),
            timeout: 2500);

        base.OnLoading();
    }

    private Drawable2d makeItem(Color c)
    {
        return new Container2d
        {
            RelativeSizeAxes = Axes.X,
            Height = 70,
            Children =
            [
                new DrawableBox2d
                {
                    Size = new Vector2(480, 70),
                    Color = c,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            ]
        };
    }

    private Drawable2d makeAnimatedItem()
    {
        return itemWrapper = new Container2d
        {
            RelativeSizeAxes = Axes.X,
            Height = 90,
            Children =
            [
                new DrawableBox2d
                {
                    Size = new Vector2(240, 70),
                    Color = Color.SkyBlue,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                itemBox = new DrawableBox2d
                {
                    Size = new Vector2(70, 70),
                    Color = Color.Gold,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            ]
        };
    }
}
