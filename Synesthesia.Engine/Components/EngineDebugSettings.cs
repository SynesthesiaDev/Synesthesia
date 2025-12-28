using Common.Util;
using Synesthesia.Engine.Components.Two;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;

namespace Synesthesia.Engine.Components;

public class EngineDebugSettings : CompositeDrawable2d
{
    protected override void OnLoading()
    {
        AutoSizeAxes = Axes.Both;
        Children =
        [
            new FillFlowContainer2d
            {
                Direction = Direction.Vertical,
                Spacing = 5,
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new DefaultEngineButton
                    {
                        Text = "Run GC",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        OnClick = GC.Collect
                    },
                ]
            },
        ];
        base.OnLoading();
    }
}