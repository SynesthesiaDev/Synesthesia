using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
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
        new EngineLongStatisticAtomicLine("Drawables", EngineStatistics.DRAWABLES),
        new EngineLongStatisticAtomicLine("Layout Invalidations", EngineStatistics.LAYOUT_INVALIDATIONS),
        new Spacer(),
        new EngineLongStatisticAtomicLine("Texture uploads", EngineStatistics.TEXTURE_UPLOADS),
        new EngineLongStatisticAtomicLine("Shader binds", EngineStatistics.SHADER_BINDS),
        new Spacer(),
        new EngineLongStatisticAtomicLine("BindablePools", EngineStatistics.BINDABLE_POOLS),
        new EngineLongStatisticAtomicLine("Bindables Borrowed", EngineStatistics.BINDABLES_BORROWED),
        new EngineLongStatisticAtomicLine("Dispatchers", EngineStatistics.DISPATCHERS),
        new Spacer(),
        new EngineStatisticLongLine("GC Collections Gen0", () => GC.CollectionCount(0)),
        new EngineStatisticLongLine("GC Collections Gen1", () => GC.CollectionCount(1)),
        new EngineStatisticLongLine("GC Collections Gen2", () => GC.CollectionCount(2)),
        new EngineStatisticLongLine("GC Size Gen0", () => gcMemoryInfo.GenerationInfo[0].SizeAfterBytes),
        new EngineStatisticLongLine("GC Size Gen1", () => gcMemoryInfo.GenerationInfo[1].SizeAfterBytes),
        new EngineStatisticLongLine("GC Size Gen2", () => gcMemoryInfo.GenerationInfo[2].SizeAfterBytes),
        new Spacer(),
        new EngineLongStatisticAtomicLine("Schedulers", EngineStatistics.SCHEDULERS),
        new EngineLongStatisticAtomicLine("Scheduler Tasks", EngineStatistics.SCHEDULER_TASKS),
        new Spacer(),
        new EngineLongStatisticAtomicLine("Animators", EngineStatistics.ANIMATORS),
        new EngineLongStatisticAtomicLine("Animations", EngineStatistics.ANIMATIONS),
        new Spacer(),
        new EngineLongStatisticAtomicLine("Audio Channels", EngineStatistics.AUDIO_CHANNELS),
        new EngineLongStatisticAtomicLine("Audio Mixers", EngineStatistics.AUDIO_MIXERS),
        new EngineLongStatisticAtomicLine("Audio Sample Instances", EngineStatistics.AUDIO_SAMPLE_INSTANCES),
        new EngineDoubleStatisticAtomicLine("BASS Cpu %", EngineStatistics.BASS_CPU),
        new Spacer(),
        new EngineLongStatisticAtomicLine("Dependencies Resolved", EngineStatistics.DEPENDENCIES_RESOLVED),
        new Spacer(),
        new EngineLongStatisticAtomicLine("Objects Rented", EngineStatistics.OBJECTS_RENTED),
        new EngineLongStatisticAtomicLine("Objects Returned", EngineStatistics.OBJECTS_RETURNED),
        new EngineLongStatisticAtomicLine("Objects Alive", EngineStatistics.OBJECTS_ALIVE)
    );

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if (!Visible) return;
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

    private class EngineStatisticLongLine(string name, Func<long> valueSource) : EngineStatisticLine
    {
        private readonly StringBuilder formatBuffer = new(16);

        private long lastValue = long.MinValue;
        private Text2d valueText = null!;

        protected override void OnLoading()
        {
            Size = new Vector2(330, 24);
            Children =
            [
                new Text2d
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = name
                },
                valueText = new Text2d
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Text = string.Empty
                },
            ];
        }

        private ThrottledUpdater statUpdater = new(100);

        protected internal override void OnUpdate(FrameInfo frameInfo)
        {
            if (statUpdater.TryUpdate(frameInfo.Delta))
            {
                var currentValue = valueSource();

                if (currentValue != lastValue)
                {
                    lastValue = currentValue;
                    updateText(currentValue);
                }
            }

            base.OnUpdate(frameInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void updateText(long value)
        {
            formatBuffer.Clear();
            formatBuffer.AppendFormat("{0:##,##0}", value);
            valueText.Text = formatBuffer.ToString();
        }
    }

    private class EngineStatisticDoubleLine(string name, Func<double> valueSource) : EngineStatisticLine
    {
        private readonly StringBuilder formatBuffer = new(16);

        private double lastValue = double.MinValue;
        private Text2d valueText = null!;

        private const double epsilon = 0.01;

        protected override void OnLoading()
        {
            Size = new Vector2(330, 24);
            Children =
            [
                new Text2d
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = name
                },
                valueText = new Text2d
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = string.Empty
                },
            ];
        }

        private ThrottledUpdater statUpdater = new(100);

        protected internal override void OnUpdate(FrameInfo frameInfo)
        {
            if (statUpdater.TryUpdate(frameInfo.Delta))
            {
                var currentValue = valueSource();

                if (Precision.IsSame(currentValue, lastValue, epsilon))
                {
                    lastValue = currentValue;
                    updateText(currentValue);
                }
            }

            base.OnUpdate(frameInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void updateText(double value)
        {
            valueText.Text = value.ToString("N2");
        }
    }

    private class EngineLongStatisticAtomicLine(string name, AtomicInt counter) : EngineStatisticLongLine(name, () => counter.Value);

    private class EngineDoubleStatisticAtomicLine(string name, Atomic<double> counter) : EngineStatisticDoubleLine(name, () => counter.Value);
}
