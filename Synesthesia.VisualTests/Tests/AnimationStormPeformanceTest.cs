using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.VisualTests.Tests;

public class AnimationStormPerformanceTest : VisualTest
{
    private Container2d content = null!;
    private readonly List<DrawableBox2d> boxes = [];

    private static bool near(float a, float b, float eps = 1.25f) => MathF.Abs(a - b) <= eps;

    protected override void OnLoading()
    {
        Children =
        [
            new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    content = new Container2d
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                ]
            }
        ];

        AddStep("Build 600 boxes", () => build(600), runNextImmediately: true);
        AddAssert("Built", () => boxes.Count == 600);

        AddWaitUntil("Wait until boxes loaded", () => boxes.Count == 600 && boxes.All(b => b.IsLoaded), timeout: 8000);

        AddStep("Start animations (move+fade+scale)", () =>
        {
            for (int i = 0; i < boxes.Count; i++)
            {
                var b = boxes[i];

                var targetX = 40f + (i % 40) * 14f;
                var targetY = 40f + (i / 40) * 14f;

                b.MoveTo(new Vector2(targetX, targetY), 650, Easing.OutCubic);
                b.ScaleTo(1.15f, 650, Easing.OutQuad);
                b.FadeTo(0.35f, 650, Easing.OutQuad);
            }
        }, runNextImmediately: true);

        AddWaitUntil("Wait until samples reached target", () =>
        {
            if (boxes.Count < 600) return false;
            return sampleOk(0) && sampleOk(123) && sampleOk(599);
        }, timeout: 8000);

        AddAssert("Samples in expected end state", () => sampleOk(0) && sampleOk(123) && sampleOk(599));

        AddStep("Restore (reverse)", () =>
        {
            foreach (var b in boxes)
            {
                b.MoveTo(Vector2.Zero, 500, Easing.OutCubic);
                b.ScaleTo(1f, 500, Easing.OutCubic);
                b.FadeTo(1f, 350, Easing.OutQuad);
            }
        }, runNextImmediately: true);

        AddWaitUntil("Wait until samples restored", () => restoredSampleOk(0) && restoredSampleOk(123) && restoredSampleOk(599), timeout: 8000);
        AddAssert("Samples restored", () => restoredSampleOk(0) && restoredSampleOk(123) && restoredSampleOk(599));

        base.OnLoading();
    }

    private void build(int count)
    {
        boxes.Clear();

        var list = new List<DrawableBox2d>(count);
        for (int i = 0; i < count; i++)
        {
            var b = new DrawableBox2d
            {
                Size = new Vector2(10, 10),
                Position = Vector2.Zero,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Color = (i % 3 == 0) ? Color.SkyBlue : (i % 3 == 1 ? Color.Gold : Color.Lime)
            };
            boxes.Add(b);
            list.Add(b);
        }

        content.Children = list;
    }

    private bool sampleOk(int index)
    {
        var b = boxes[index];

        var targetX = 40f + (index % 40) * 14f;
        var targetY = 40f + (index / 40) * 14f;

        return near(b.Position.X, targetX) &&
               near(b.Position.Y, targetY) &&
               b.Alpha <= 0.38f &&
               near(b.Scale.X, 1.15f, 0.05f) &&
               near(b.Scale.Y, 1.15f, 0.05f);
    }

    private bool restoredSampleOk(int index)
    {
        var b = boxes[index];

        return near(b.Position.X, 0f) &&
               near(b.Position.Y, 0f) &&
               b.Alpha >= 0.98f &&
               near(b.Scale.X, 1f, 0.05f) &&
               near(b.Scale.Y, 1f, 0.05f);
    }
}
