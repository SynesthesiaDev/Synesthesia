// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.VisualTests.Tests;

public class BorderTest : VisualTest
{
    protected override void OnLoading()
    {
        Children =
        [
            new Container2d
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                BorderThickness = 1,
                BorderColor = ComplexColor.GradientVertical(Defaults.BACKGROUND5, Color.White with { A = 0 }),
                BorderType = BorderType.Outset,
                CornerRadius = 10,
                Masking = true,
                Children =
                [
                    new Box2d
                    {
                        Size = new Vector2(300, 200),
                        Color = Defaults.BACKGROUND2
                    }
                ]
            }
        ];
        base.OnLoading();
    }
}
