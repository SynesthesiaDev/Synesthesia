// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Bindable;
using Common.Util;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.VisualTests.Tests;

public class SliderTest : VisualTest
{
    public override string Name => "Slider Component";

    private readonly BindableFloat currentValue = new()
    {
        Max = 10f,
        Min = 0f,
    };

    private TextDrawable text = null!;

    public override List<Drawable2d> Setup()
    {
         List<Drawable2d> children = [
            new FillFlowContainer2d
            {
                AutoSizeAxes = Axes.Both,
                Direction = Direction.Vertical,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = 10f,
                Children =
                [
                    text = new TextDrawable
                    {
                        Text = $"{currentValue.Value}"
                    },
                    new DefaultSliderBar
                    {
                        Current = currentValue,
                        Size = new Vector2(400, 40),
                        Precision = 0.1f,
                    }
                ]
            },
        ];

        currentValue.OnValueChange(e =>
        {
            text.Text = $"{e.NewValue}";
        });

        return children;
    }

    public override void Cleanup()
    {
        currentValue.Dispose();
    }
}
