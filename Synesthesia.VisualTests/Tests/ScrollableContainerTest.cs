// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.VisualTests.Tests;

public class ScrollableContainerTest : VisualTest
{
    public override string Name => "Scrollable Container";

    private FillFlowContainer2d contentFillFlow = null!;

    public override List<Drawable2d> Setup()
    {
        List<Drawable2d> children = [
            new ScrollableContainer
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

        return children;
    }

    public override void Cleanup()
    {
    }
}
