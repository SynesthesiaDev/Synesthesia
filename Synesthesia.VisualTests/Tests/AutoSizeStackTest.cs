using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.VisualTests.Tests;

public class AutoSizeStackTest : VisualTest
{
    private FillFlowContainer2d autoStack = null!;

    private Container2d item1 = null!;
    private Container2d item2 = null!;
    private Container2d item3 = null!;
    private Container2d? item4;

    private readonly List<Container2d> items = [];

    private const float spacing = 10f;

    private static bool near(float a, float b, float eps = 0.9f) => MathF.Abs(a - b) <= eps;

    protected override void OnLoading()
    {
        Children =
        [
            new Container2d
            {
                Size = new Vector2(900, 600),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new Box2d
                    {
                        RelativeSizeAxes = Axes.Both,
                        Color = new Color(20, 20, 20, 255),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },

                    autoStack = new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Vertical,
                        Spacing = spacing,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children =
                        [
                            item1 = makeItem(new Vector2(320, 40), Color.DarkBlue),
                            item2 = makeItem(new Vector2(280, 60), Color.DarkGreen),
                            item3 = makeItem(new Vector2(360, 50), Color.DarkPurple),
                        ]
                    }
                ]
            }
        ];

        AddStep("Reset items + sizes", () =>
        {
            items.Clear();
            items.AddRange([item1, item2, item3]);

            if (item4 != null)
            {
                autoStack.RemoveChild(item4);
                item4 = null;
            }

            item1.Size = new Vector2(320, 40);
            item2.Size = new Vector2(280, 60);
            item3.Size = new Vector2(360, 50);
        }, runNextImmediately: true);

        AddWaitUntil("Wait for initial layout", expectedMatchesCurrent, timeout: 1500);

        AddAssert("AutoSize stack size matches children", expectedMatchesCurrent,
            "Expected: width=max(child widths), height=sum(child heights)+spacing*(n-1).");

        AddStep("Resize middle item height to 110", () =>
        {
            item2.ResizeHeightTo(110f, 450, Easing.OutCubic);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until middle item resized", () => near(item2.Height, 110f, 1.0f), timeout: 2000);
        AddWaitUntil("Wait until stack size updated", expectedMatchesCurrent, timeout: 2000);
        AddAssert("AutoSize updated after child resize", expectedMatchesCurrent);

        AddStep("Add a new child item", () =>
        {
            if (item4 != null) return;

            item4 = makeItem(new Vector2(300, 80), Color.Maroon);
            autoStack.AddChild(item4);
            items.Add(item4);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until stack grew after add", expectedMatchesCurrent, timeout: 2000);
        AddAssert("AutoSize grew correctly", expectedMatchesCurrent);

        AddStep("Remove the new child item", () =>
        {
            if (item4 == null) return;

            autoStack.RemoveChild(item4);
            items.Remove(item4);
            item4 = null;
        }, runNextImmediately: true);

        AddWaitUntil("Wait until stack shrank after remove", expectedMatchesCurrent, timeout: 2000);
        AddAssert("AutoSize shrank correctly", expectedMatchesCurrent);

        base.OnLoading();
    }

    private static Container2d makeItem(Vector2 size, Color c)
    {
        return new Container2d
        {
            Size = size,
            Children =
            [
                new Box2d
                {
                    RelativeSizeAxes = Axes.Both,
                    Color = c,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            ]
        };
    }

    private bool expectedMatchesCurrent()
    {
        if (items.Count == 0) return near(autoStack.Width, 0f) && near(autoStack.Height, 0f);

        var expectedW = items.Max(i => i.Width);
        var expectedH = items.Sum(i => i.Height) + spacing * (items.Count - 1);

        return near(autoStack.Width, expectedW) && near(autoStack.Height, expectedH);
    }
}
