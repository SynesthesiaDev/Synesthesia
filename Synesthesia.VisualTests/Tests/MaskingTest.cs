// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Bindable;
using Common.Util;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Resources;

namespace Synesthesia.VisualTests.Tests;

public class MaskingTest : VisualTest
{
    [Resolved]
    private IResourceStore<Texture> textureStore = null!;

    private readonly Bindable<bool> masking = new(true);

    private readonly BindableFloat cornerRadius = new()
    {
        Min = 0,
        Max = 200,
        Default = 10
    };

    private Container2d container2d = null!;

    protected override void OnLoading()
    {
        Children =
        [
            new FillFlowContainer2d
            {
                AutoSizeAxes = Axes.Both,
                Direction = Direction.Vertical,
                Spacing = 10f,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    container2d = new Container2d
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(600, 400),
                        Masking = true,
                        CornerRadius = 20f,
                        Children =
                        [
                            new Box2d
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Texture = textureStore.Get("Synesthesia.Resources.test_img_big.png"),
                                RelativeSizeAxes = Axes.Both,
                                TextureFillMode = TextureFillMode.Fill,
                            }
                        ]
                    },
                    new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Vertical,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Spacing = 10f,
                        Children =
                        [
                            new DefaultCheckbox
                            {
                                Checked = masking,
                                Text = "Masking",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Size = new Vector2(350, 40),
                            },
                            new LabelledSliderBar
                            {
                                Size = new Vector2(350, 40),
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                SliderBar = new DefaultSliderBar
                                {
                                    Current = cornerRadius,
                                    Size = new Vector2(240, 40),
                                },
                                Label = "Corner Radius"
                            },
                        ]
                    },
                ]
            },
        ];

        base.OnLoading();
    }

    protected override void LoadComplete()
    {
        masking.OnValueChange(e => container2d.Masking = e.NewValue, true);
        cornerRadius.OnValueChange(e => container2d.CornerRadius = e.NewValue, true);

        base.LoadComplete();
    }

    protected override void Dispose(bool isDisposing)
    {
        masking.Dispose();
        cornerRadius.Dispose();

        base.Dispose(isDisposing);
    }
}
