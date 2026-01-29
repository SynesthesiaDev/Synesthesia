using System.Collections.Immutable;
using System.Numerics;
using Common.Statistics;
using Common.Util;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using SynesthesiaUtil;
using SynesthesiaUtil.Types;

namespace Synesthesia.Engine.Components.Two.Debug;

public class EngineStatisticsPanel : EngineDebugComponent
{
    private static GCMemoryInfo gcMemoryInfo => GC.GetGCMemoryInfo();

    private readonly ImmutableList<EngineStatisticLine> statistics = Lists.Immutable<EngineStatisticLine>
    (
        new EngineStatisticAtomicLine("Drawables", EngineStatistics.DRAWABLES),
        new Spacer(),
        new EngineStatisticAtomicLine("BindablePools", EngineStatistics.BINDABLE_POOLS),
        new EngineStatisticAtomicLine("Bindables Borrowed", EngineStatistics.BINDABLES_BORROWED),
        new EngineStatisticAtomicLine("Dispatchers", EngineStatistics.DISPATCHERS),
        new EngineStatisticAtomicLine("Dispatcher Borrowed", EngineStatistics.DISPATCHERS_BORROWED),
        new Spacer(),
        new EngineStatisticTextLine("GC Collections Gen0", () => GC.CollectionCount(0)),
        new EngineStatisticTextLine("GC Collections Gen1", () => GC.CollectionCount(1)),
        new EngineStatisticTextLine("GC Collections Gen2", () => GC.CollectionCount(2)),
        new EngineStatisticTextLine("GC Size Gen0", () => gcMemoryInfo.GenerationInfo[0].SizeAfterBytes),
        new EngineStatisticTextLine("GC Size Gen1", () => gcMemoryInfo.GenerationInfo[1].SizeAfterBytes),
        new EngineStatisticTextLine("GC Size Gen2", () => gcMemoryInfo.GenerationInfo[2].SizeAfterBytes),
        new EngineStatisticTextLine("Finalization Queue", () => gcMemoryInfo.FinalizationPendingCount),
        new Spacer(),
        new EngineStatisticAtomicLine("Schedulers (Lazy)", EngineStatistics.SCHEDULERS),
        new EngineStatisticAtomicLine("Schedulers (Active)", EngineStatistics.ACTIVE_SCHEDULERS),
        new EngineStatisticAtomicLine("Scheduler Tasks", EngineStatistics.SCHEDULER_TASKS),
        new Spacer(),
        new EngineStatisticAtomicLine("Animators (Lazy)", EngineStatistics.ANIMATORS),
        new EngineStatisticAtomicLine("Animators (Active)", EngineStatistics.ACTIVE_ANIMATORS),
        new EngineStatisticAtomicLine("Animations", EngineStatistics.ANIMATIONS),
        new Spacer(),
        new EngineStatisticAtomicLine("Audio Channels", EngineStatistics.AUDIO_CHANNELS),
        new EngineStatisticAtomicLine("Audio Mixers", EngineStatistics.AUDIO_MIXERS),
        new EngineStatisticAtomicLine("Cached Audio Samples", EngineStatistics.CACHED_AUDIO_SAMPLES),
        new EngineStatisticAtomicLine("Audio Sample Instances", EngineStatistics.AUDIO_SAMPLE_INSTANCES),
        new EngineStatisticAtomicLine("BASS Cpu %", EngineStatistics.BASS_CPU)
    );

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if(!Visible) return;
        base.OnUpdate(frameInfo);
    }


    protected override void OnLoading()
    {
        AutoSizeAxes = Axes.Both;
        Children =
        [
            new BackgroundContainer2d
            {
                AutoSizeAxes = Axes.Both,
                BackgroundColor = Defaults.BACKGROUND2,
                BackgroundCornerRadius = 10f,
                AutoSizePadding = new Vector4(10),
                Children =
                [
                    new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = [..statistics.ToList()],
                        Direction = Direction.Vertical
                    },
                ]
            }
        ];
    }

    private abstract class EngineStatisticLine : CompositeDrawable2d;

    private class Spacer : EngineStatisticLine
    {
        protected override void OnLoading()
        {
            Size = new Vector2(1, 16);
        }
    }

    private class EngineStatisticTextLine(string name, Func<string> statisticGetter) : EngineStatisticLine
    {
        public EngineStatisticTextLine(string name, Func<long> longGetter) : this(name, () => $"{longGetter.Invoke():##,##0}")
        {

        }

        protected override void OnLoading()
        {
            Size = new Vector2(250, 24);
            Children =
            [
                new TextDrawable
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = name
                },
                new FrameUpdatableTextDrawable
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    UpdateOnDraw = () => $"{statisticGetter.Invoke()}"
                },
            ];
        }
    }

    private class EngineStatisticAtomicLine(string name, IAtomic counter) : EngineStatisticTextLine(name, counter.GetValueAsString);
}
