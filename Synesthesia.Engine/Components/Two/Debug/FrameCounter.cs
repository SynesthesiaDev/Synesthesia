// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util;

namespace Synesthesia.Engine.Components.Two.Debug;

public class FrameCounter : CompositeDrawable2d
{
    [Singleton]
    private Game game = null!;

    protected override void OnLoading()
    {
        Size = new Vector2(450, 150);
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
                        Children =
                        [
                            new PerformanceMonitorElement
                            {
                                Name = "Draw",
                                Fps = () => game.RenderThread.Fps,
                                MaxFps = game.RenderThread.ActiveUpdateRate.Value,
                                FrameTime = () => game.RenderThread.FrameTime
                            },
                            new PerformanceMonitorElement
                            {
                                Name = "Update",
                                Fps = () => game.UpdateThread.Fps,
                                MaxFps = game.UpdateThread.ActiveUpdateRate.Value,
                                FrameTime = () => game.UpdateThread.FrameTime
                            },
                            new PerformanceMonitorElement
                            {
                                Name = "Input",
                                Fps = () => game.InputThread.Fps,
                                MaxFps = game.InputThread.ActiveUpdateRate.Value,
                                FrameTime = () => game.InputThread.FrameTime
                            },
                            new PerformanceMonitorElement
                            {
                                Name = "Audio",
                                Fps = () => game.AudioThread.Fps,
                                MaxFps = game.AudioThread.ActiveUpdateRate.Value,
                                FrameTime = () => game.AudioThread.FrameTime
                            },
                        ],
                        Direction = Direction.Vertical
                    }
                ]
            }
        ];
    }

    private class PerformanceMonitorElement : CompositeDrawable2d
    {
        public string Name { get; init; } = "Element";
        public Func<double> Fps { get; init; } = () => 0d;
        public Func<double> FrameTime { get; init; } = () => 0d;

        private double lastFps;
        private double lastFrameTime;

        public long MaxFps { get; init; }

        private Text2d fpsText = null!;
        private Text2d frameTimeText = null!;

        protected override void OnLoading()
        {
            Size = new Vector2(400, 30);

            Children =
            [
                new Container2d
                {
                    RelativeSizeAxes = Axes.Both,
                    Children =
                    [
                        new Text2d
                        {
                            Text = $"{Name}:",
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Color = EngineBranding.TEXT0
                        },
                        new Container2d
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreRight,
                            AutoSizeAxes = Axes.Both,
                            Children =
                            [
                                fpsText = new Text2d
                                {
                                    Text = string.Empty,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreRight,
                                    Color = EngineBranding.TEXT2
                                },
                                new Text2d
                                {
                                    Text = $" / {MaxFps} fps",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreLeft,
                                    Color = EngineBranding.TEXT2
                                },
                            ]
                        },
                        frameTimeText = new Text2d
                        {
                            Text = "(0.000 ms)",
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Color = EngineBranding.TEXT1
                        },
                    ]
                }
            ];
        }

        private ThrottledUpdater throttledUpdater = new(100);

        protected internal override void OnUpdate(FrameInfo frameInfo)
        {
            if (throttledUpdater.TryUpdate(frameInfo.Delta))
            {
                var currentFps = Fps();
                var currentFrameTime = FrameTime();

                if (!Precision.IsSame(currentFps, lastFps))
                {
                    lastFps = currentFps;
                    fpsText.Text = $"{currentFps:0}";
                }

                if (!Precision.IsSame(currentFrameTime, lastFrameTime, 0.001))
                {
                    lastFrameTime = currentFrameTime;
                    frameTimeText.Text = $"({currentFrameTime:0.000} ms)";
                }
            }

            base.OnUpdate(frameInfo);
        }
    }
}
