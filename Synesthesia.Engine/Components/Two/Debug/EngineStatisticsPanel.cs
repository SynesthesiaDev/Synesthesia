// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util;
using Synesthesia.Engine.Util.Statistics;
using SynesthesiaUtil;

namespace Synesthesia.Engine.Components.Two.Debug;

public class EngineStatisticsPanel : EngineDebugElement
{
    private static GCMemoryInfo gcMemoryInfo => GC.GetGCMemoryInfo();

    private readonly ImmutableList<Drawable2d> statistics = Lists.Immutable<Drawable2d>
    (
        new HeaderComponent("Statistics Panel (Ctrl + F2)"),
        new EngineStatisticLine(EngineStatistics.Type.Drawables),
        new DrawStatisticLine(DrawStatistics.Type.Invalidations),
        new Spacer(),
        new DrawStatisticLine(DrawStatistics.Type.DrawCalls),
        new DrawStatisticLine(DrawStatistics.Type.TextureBinds),
        new DrawStatisticLine(DrawStatistics.Type.ShaderBinds),
        new DrawStatisticLine(DrawStatistics.Type.TextureUploadQueue),
        new DrawStatisticLine(DrawStatistics.Type.UniformUploads),
        new DrawStatisticLine(DrawStatistics.Type.VertexBatchFlushes),
        new DrawStatisticLine(DrawStatistics.Type.VertexBatchOverflows),
        new Spacer(),
        new StatisticLine("GC Collections Gen0", () => GC.CollectionCount(0)),
        new StatisticLine("GC Size Gen0", () => gcMemoryInfo.GenerationInfo[0].SizeAfterBytes),
        new Spacer(),
        new EngineStatisticLine(EngineStatistics.Type.ObjectsInflight),
        new EngineStatisticLine(EngineStatistics.Type.ObjectsRented),
        new EngineStatisticLine(EngineStatistics.Type.ObjectsReturned),
        new Spacer(),
        new EngineStatisticLine(EngineStatistics.Type.TextureAtlases),
        new EngineStatisticLine(EngineStatistics.Type.FontAtlases),
        new EngineStatisticLine(EngineStatistics.Type.CachedResourceStoreSize),
        new EngineStatisticLine(EngineStatistics.Type.ColorCacheSize),
        new EngineStatisticLine(EngineStatistics.Type.DependenciesResolved)
    );

    protected override void OnLoading()
    {
        var height = statistics.Count * 20 + 20;
        Size = new Vector2(310, height);
        Children =
        [
            new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new Box2d
                    {
                        RelativeSizeAxes = Axes.Both,
                        Color = EngineBranding.BACKGROUND2,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        CornerRadius = 10,
                    },
                    new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = [..statistics.ToList()],
                        Direction = Direction.Vertical
                    }
                ]
            }
        ];
    }

    private abstract class Line : CompositeDrawable2d;

    private class EngineStatisticLine(EngineStatistics.Type type) : StatisticLine(type.ToString(), () => EngineStatistics.Get(type));

    private class DrawStatisticLine(DrawStatistics.Type type) : StatisticLine(type.ToString(), () => DrawStatistics.Get(type));

    private class Spacer : Line
    {
        protected override void OnLoading()
        {
            Size = new Vector2(1, 16);
        }
    }

    private class StatisticLine(string name, Func<long> valueSource) : Line
    {
        private readonly StringBuilder formatBuffer = new(16);

        private long lastValue = long.MinValue;
        private Text2d valueText = null!;

        protected override void OnLoading()
        {
            Size = new Vector2(270, 20);
            Children =
            [
                new Text2d
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = name,
                    Color = EngineBranding.TEXT1
                },
                valueText = new Text2d
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Text = string.Empty,
                    Color = EngineBranding.TEXT2
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
}
