using System.Collections.Immutable;
using System.Numerics;
using Common.Statistics;
using Common.Util;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using SynesthesiaUtil;
using SynesthesiaUtil.Types;

namespace Synesthesia.Engine.Components.Two.Debug;

public class EngineStatisticsPanel : CompositeDrawable2d
{
    private static GCMemoryInfo GcMemoryInfo => GC.GetGCMemoryInfo();

    private ImmutableList<IEngineStatisticLine> _statistics = Lists.Immutable<IEngineStatisticLine>
    (
        new EngineStatisticAtomicLine("Drawables", EngineStatistics.Drawables),
        new Spacer(),
        new EngineStatisticAtomicLine("BindablePools", EngineStatistics.BindablePools),
        new EngineStatisticAtomicLine("Bindables Borrowed", EngineStatistics.BindablesBorrowed),
        new EngineStatisticAtomicLine("Dispatchers", EngineStatistics.Dispatchers),
        new EngineStatisticAtomicLine("Dispatcher Borrowed", EngineStatistics.DispatchersBorrowed),
        new Spacer(),
        new EngineStatisticTextLine("GC Collections Gen0", () => GC.CollectionCount(0)),
        new EngineStatisticTextLine("GC Collections Gen1", () => GC.CollectionCount(1)),
        new EngineStatisticTextLine("GC Collections Gen2", () => GC.CollectionCount(2)),
        new EngineStatisticTextLine("GC Size Gen0", () => GcMemoryInfo.GenerationInfo[0].SizeAfterBytes),
        new EngineStatisticTextLine("GC Size Gen1", () => GcMemoryInfo.GenerationInfo[1].SizeAfterBytes),
        new EngineStatisticTextLine("GC Size Gen2", () => GcMemoryInfo.GenerationInfo[2].SizeAfterBytes),
        new EngineStatisticTextLine("Finalization Queue", () => GcMemoryInfo.FinalizationPendingCount),
        new Spacer(),
        new EngineStatisticAtomicLine("Schedulers", EngineStatistics.Schedulers),
        new EngineStatisticAtomicLine("Scheduler Tasks", EngineStatistics.SchedulerTasks)
    );

    protected override void OnLoading()
    {
        AutoSizeAxes = Axes.Both;
        Children =
        [
            new BackgroundContainer2d
            {
                AutoSizeAxes = Axes.Both,
                BackgroundColor = Defaults.Background2,
                BackgroundAlpha = 0.9f,
                BackgroundCornerRadius = 10f,
                AutoSizePadding = new Vector4(10),
                Children =
                [
                    new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = [.._statistics.ToList()],
                        Direction = Direction.Vertical
                    },
                ]
            }
        ];
    }

    private abstract class IEngineStatisticLine : CompositeDrawable2d;

    private class Spacer : IEngineStatisticLine
    {
        protected override void OnLoading()
        {
            Size = new Vector2(1, 16);
        }
    }

    private class EngineStatisticTextLine(string Name, Func<long> StatisticGetter) : IEngineStatisticLine
    {
        protected override void OnLoading()
        {
            Size = new Vector2(250, 24);
            Children =
            [
                new TextDrawable
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = Name
                },
                new FrameUpdatableTextDrawable
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    UpdateOnDraw = () => $"{StatisticGetter.Invoke():##,##0}"
                }
            ];
        }
    }

    private class EngineStatisticAtomicLine(string Name, AtomicInt Counter) : EngineStatisticTextLine(Name, () => Counter.Value);
}