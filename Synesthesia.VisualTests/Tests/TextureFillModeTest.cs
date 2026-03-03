// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Resources;

namespace Synesthesia.VisualTests.Tests;

public class TextureFillModeTest : VisualTest
{
    private Box2d box2d = null!;

    [Resolved]
    private IResourceStore<Texture> textureStore = null!;

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
                    new BackgroundContainer2d
                    {
                        Size = new Vector2(200, 100),
                        BackgroundColor = Defaults.BACKGROUND2,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,

                        Children =
                        [
                            box2d = new Box2d
                            {
                                RelativeSizeAxes = Axes.Both,
                                Texture = textureStore.Get("Synesthesia.Resources.dull_blade.bmp"),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre
                            }
                        ]
                    },

                    new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Horizontal,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Spacing = 10f,
                        Children =
                        [
                            new TextureFillModeButton(TextureFillMode.Stretch, box2d),
                            new TextureFillModeButton(TextureFillMode.Fill, box2d),
                            new TextureFillModeButton(TextureFillMode.Fit, box2d),
                        ]
                    }
                ]
            },
        ];

        base.OnLoading();
    }

    private class TextureFillModeButton : DefaultButton
    {
        public TextureFillModeButton(TextureFillMode fillMode, Box2d box2d)
        {
            ColorCombination = DefaultEngineColorCombination.ACCENT;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            Size = new Vector2(120, 40);

            TextColor = Color.Black;

            Text = fillMode.ToString();
            OnClick = () => box2d.TextureFillMode = fillMode;
        }
    }
}
