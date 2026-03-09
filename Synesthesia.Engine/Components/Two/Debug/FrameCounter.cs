using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.Engine.Components.Two.Debug;

public class FrameCounter : EngineDebugComponent
{
    [Resolved]
    private Game game = null!;

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
                BackgroundAlpha = 1f,
                BackgroundCornerRadius = 10f,
                AutoSizePadding = new Vector4(10),
                Children =
                [
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
                                Fps = () => Raylib.GetFPS(),
                                MaxFps = Defaults.RENDERER_RATE,
                                FrameTime = () => Raylib.GetFrameTime()
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
            Size = new Vector2(330, 24);
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
                                    Origin = Anchor.CentreRight
                                },
                                new Text2d
                                {
                                    Text = $" / {MaxFps} fps",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreLeft
                                },
                            ]
                        },
                        frameTimeText = new Text2d
                        {
                            Text = "(0.000 ms)",
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight
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
                // var currentFps = Math.Clamp(Fps(), 0, MaxFps);
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
