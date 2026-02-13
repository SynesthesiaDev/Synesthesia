// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Util;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Container;

public class ParallaxContainer : Container2d
{
    public float Strength { get; set; } = 0.05f;

    private Container2d content = null!;

    protected override void OnLoading()
    {
        Children =
        [
            content = new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            }
        ];

        base.OnLoading();
    }
}
