// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Util;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Resources;

namespace Synesthesia.VisualTests.Tests;

public class TextureBigTest : VisualTest
{
    [Resolved]
    private IResourceStore<Texture> textureStore = null!;

    protected override void OnLoading()
    {
        Children =
        [
            new Box2d
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Texture = textureStore.Get("Synesthesia.Resources.test_img_big.png"),
                TextureFillMode = TextureFillMode.Fit
            }
        ];
        base.OnLoading();
    }
}
