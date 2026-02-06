using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.VisualTests.Tests;

public class DrawableSpawnPerformanceTest : VisualTest
{
    private Container2d root = null!;
    private Container2d content = null!;

    private const int default_count = 1500;

    protected override void OnLoading()
    {
        Children =
        [
            root = new Container2d
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

        AddStep("Clear content", () => content.Children = [], runNextImmediately: true);
        AddAssert("Content empty", () => !content.Children.Any());

        AddStep($"Spawn {default_count} boxes", () => spawnGrid(default_count));
        AddAssert("Boxes spawned", () => content.Children.Count() == default_count);

        AddStep("Spawn 3000 boxes", () => spawnGrid(3000));
        AddAssert("Boxes spawned (3000)", () => content.Children.Count() == 3000);

        AddStep("Back to 500 boxes", () => spawnGrid(500));
        AddAssert("Boxes spawned (500)", () => content.Children.Count() == 500);

        base.OnLoading();
    }

    private void spawnGrid(int count)
    {
        // Rebuild list to avoid incremental layout overhead patterns.
        var list = new List<DrawableBox2d>(count);

        var cols = 60;
        var size = 10f;
        var pad = 2f;

        for (int i = 0; i < count; i++)
        {
            var x = i % cols;
            var y = i / cols;

            list.Add(new DrawableBox2d
            {
                Size = new Vector2(size, size),
                Position = new Vector2(x * (size + pad), y * (size + pad)),
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Color = (i % 2 == 0) ? Color.DarkBlue : Color.DarkGreen
            });
        }

        content.Children = list;
    }
}
