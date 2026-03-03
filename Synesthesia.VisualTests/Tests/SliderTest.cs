// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Bindable;
using Common.Util;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using Synesthesia.Engine.Input;

namespace Synesthesia.VisualTests.Tests;

public class SliderTest : VisualTest
{
    private readonly BindableFloat currentValue = new()
    {
        Max = 10f,
        Min = 0f,
    };

    private Text2d text = null!;
    private DefaultSliderBar sliderBar = null!;

    protected override void OnLoading()
    {
        Children =
        [
            new FillFlowContainer2d
            {
                AutoSizeAxes = Axes.Both,
                Direction = Direction.Vertical,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = 10f,
                Children =
                [
                    text = new Text2d
                    {
                        Text = $"{currentValue.Value}"
                    },
                    sliderBar = new DefaultSliderBar
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

        AddAssert("Is 0", () => currentValue.Value == 0.0);

        AddStep("Click in the middle", () =>
        {
            InputSimulator.SimulateMove(sliderBar.GetScreenSpaceCenter());
            InputSimulator.SimulateClick();
        });

        AddAssert("Is 5", () => Precision.IsSame(currentValue.Value, 5.0));

        AddStep("Manually set to 0.0", () => currentValue.Value = 0.0f);

        AddAssert("Is 0.0", () => Precision.IsSame(currentValue.Value, 0.0));

        AddStep("Drag to center", () => InputSimulator.SimulateDrag(sliderBar.Nub.GetScreenSpaceCenter(), sliderBar.Body.GetScreenSpaceCenter(), 1000), true);

        AddWaitUntil("Wait for drag", () => Precision.IsSame(currentValue.Value, 5.0));

        AddAssert("Is 5", () => Precision.IsSame(currentValue.Value, 5.0));
    }

    protected override void Dispose(bool isDisposing)
    {
        currentValue.Dispose();
        base.Dispose(isDisposing);
    }
}
