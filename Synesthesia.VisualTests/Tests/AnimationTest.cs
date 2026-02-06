// Copyright (c) 2026 SynesthesiaDev <...>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.VisualTests.Tests;

public class AnimationTest : VisualTest
{
    private DrawableBox2d box = null!;

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
                    box = new DrawableBox2d
                    {
                        Size = new Vector2(120, 120),
                        Color = Color.SkyBlue,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                ]
            }
        ];

        AddStep("Reset box state", () =>
        {
            box.Position = Vector2.Zero;
            box.Scale = Vector2.One;
            box.Alpha = 1f;
        }, runNextImmediately: true);

        AddAssert("Initial position is (0,0)", () => Precision.IsSame(box.Position.X, 0f) && Precision.IsSame(box.Position.Y, 0f));
        AddAssert("Initial alpha is 1", () => Precision.IsSame(box.Alpha, 1f));
        AddAssert("Initial scale is 1", () => Precision.IsSame(box.Scale.X, 1f) && Precision.IsSame(box.Scale.Y, 1f));

        AddStep("Animate move to the right", () =>
        {
            box.MoveTo(new Vector2(240, 0), 600, Easing.OutCubic);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until moved", () =>
            Precision.IsSame(box.Position.X, 240f) && Precision.IsSame(box.Position.Y, 0f), 1500);

        AddAssert("Ended at x=240", () => Precision.IsSame(box.Position.X, 240f));

        AddStep("Fade out", () =>
        {
            box.FadeTo(0f, 400, Easing.OutQuad);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until invisible", () => box.Alpha <= 0.01f);
        AddAssert("Alpha is ~0", () => box.Alpha <= 0.01f);

        AddStep("Fade in + scale up", () =>
        {
            box.FadeTo(1f, 200, Easing.OutQuad);
            box.ScaleTo(1.8f, 700, Easing.OutElastic);
        }, runNextImmediately: true);

        AddWaitUntil("Wait until scaled", () =>
            Precision.IsSame(box.Scale.X, 1.8f) && Precision.IsSame(box.Scale.Y, 1.8f));

        AddAssert("Scale is 1.8", () => Precision.IsSame(box.Scale.X, 1.8f) && Precision.IsSame(box.Scale.Y, 1.8f));

        base.OnLoading();
    }
}
