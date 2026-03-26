// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Components.Two.Debug;

public class EngineDebugOverlay : Container2d
{
    [Singleton]
    private Game game = null!;

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if (!Visible) return;
        base.OnUpdate(frameInfo);
    }

    private Container2d mainContainer = null!;

    protected override void OnLoading()
    {
        Visible = true;
        Children =
        [
            mainContainer = new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Children =
                [
                    new FillFlowContainer2d
                    {
                        Position = new Vector2(10, 10),
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Vertical,
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Spacing = 10f,
                        Children =
                        [
                            new FrameCounter
                            {
                                Scale = new Vector2(0.8f),
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                            },
                        ]
                    },
                ]
            },
        ];

        base.OnLoading();
    }
}
