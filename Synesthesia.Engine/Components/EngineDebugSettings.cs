using System.Numerics;
using Common.Logger;
using Common.Util;
using Synesthesia.Engine.Components.Two;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using SynesthesiaUtil.Extensions;
using SynesthesiaUtil.Randomness;

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
                        Text = "Le Button",
                        Size = new Vector2(120, 40),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        OnClick = () =>
                        {
                            var i = RNG.RandomInt(0, 4);
                            List<Logger.LogType> type = [Logger.AUDIO, Logger.DATABASE, Logger.INPUT, Logger.IO, Logger.NETWORK, Logger.RENDER, Logger.RUNTIME];
                            switch (i)
                            {
                                case 0:
                                    Logger.Warning("Testing Test!", type.Random());
                                    break;
                                case 1:
                                    Logger.Verbose("Testing Test!", type.Random());
                                    break;
                                case 2:
                                    Logger.Error("Testing Test!", type.Random());
                                    break;
                                case 3:
                                    Logger.Debug("Testing Test!", type.Random());
                                    break;
                            }
                        }
                    },
                ]
            },
        ];
        base.OnLoading();
    }
}