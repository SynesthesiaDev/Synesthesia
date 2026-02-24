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

public class ParallaxContainerTest : VisualTest
{
    [Resolved]
    private IResourceStore<Texture> textureStore = null!;

    private ParallaxContainer parallaxContainer = null!;

    private readonly BindableFloat parallaxStrength = new()
    {
        Min = 0.01f,
        Max = 1f,
        Default = 0.05f
    };

    private readonly Bindable<bool> parallaxMasking = new(true);

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
                    parallaxContainer = new ParallaxContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(600, 400),
                        Masking = parallaxMasking.Value,
                        Content =
                        [
                            new Box2d
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Texture = textureStore.Get("Synesthesia.Resources.test_img_big.png"),
                                TextureFillMode = TextureFillMode.Fill
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
                                Text = "Enabled",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Size = new Vector2(350, 40),
                                Checked = parallaxContainer.Enabled
                            },

                            new DefaultCheckbox
                            {
                                Text = "Hover Only",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Size = new Vector2(350, 40),
                                Checked = parallaxContainer.HoverOnly
                            },

                            new DefaultCheckbox
                            {
                                Text = "Masking",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Size = new Vector2(350, 40),
                                Checked = parallaxMasking
                            },

                            new LabelledSliderBar
                            {
                                Size = new Vector2(350, 40),
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                SliderBar = new DefaultSliderBar
                                {
                                    Current = parallaxStrength,
                                    Size = new Vector2(240, 40),
                                },
                                Label = "Strength"
                            }
                        ]
                    }
                ]
            },
        ];

        parallaxStrength.OnValueChange(e => parallaxContainer.Strength = e.NewValue);
        parallaxMasking.OnValueChange(e => parallaxContainer.Masking = e.NewValue);

        base.OnLoading();
    }

    protected override void Dispose(bool isDisposing)
    {
        parallaxStrength.Dispose();
        parallaxMasking.Dispose();
        base.Dispose(isDisposing);
    }
}
