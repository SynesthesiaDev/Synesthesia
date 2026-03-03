// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
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

    protected override void OnLoading()
    {
        Children =
        [
            new Container2d
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(600, 400),
                Masking = true,
                CornerRadius = 20f,
                Children =
                [
                    new Box2d
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = textureStore.Get("Synesthesia.Resources.test_img_big.png"),
                        RelativeSizeAxes = Axes.Both,
                        TextureFillMode = TextureFillMode.Fill,
                    }
                ]
            }
        ];

        base.OnLoading();
    }
}
