using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.Engine.Components.Two;

public class FrameCounter : CompositeDrawable2d
{
    protected override void OnLoading()
    {
        var game = DependencyContainer.Get<Game>();
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
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Margin = new Vector4(10, 0, 0, 0),
                        Children =
                        [
                            new PerformanceMonitorElement
                            {
                                Name = "Draw",
                                Fps = () => Raylib.GetFPS(),
                                MaxFps = game.RenderThread.FpsTarget,
                                FrameTime = () => $"{Raylib.GetFrameTime():0.000}"
                            },
                            new PerformanceMonitorElement
                            {
                                Name = "Update",
                                Fps = () => game.UpdateThread.Fps,
                                MaxFps = game.UpdateThread.FpsTarget,
                                FrameTime = () => $"{game.UpdateThread.FrameTime.Milliseconds:0.000}"
                            },
                            new PerformanceMonitorElement
                            {
                                Name = "Input",
                                Fps = () => game.InputThread.Fps,
                                MaxFps = game.InputThread.FpsTarget,
                                FrameTime = () => $"{game.InputThread.FrameTime.Milliseconds:0.000}"
                            },
                            new PerformanceMonitorElement
                            {
                                Name = "Audio",
                                Fps = () => game.AudioThread.Fps,
                                MaxFps = game.AudioThread.FpsTarget,
                                FrameTime = () => $"{game.AudioThread.FrameTime.Milliseconds:0.000}"
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
        public Func<double> Fps { get; init; } = () => 0;
        public Func<string> FrameTime { get; init; } = () => "0.00";
        public int MaxFps { get; init; }

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
                        new TextDrawable
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
                                new FrameUpdatableTextDrawable
                                {
                                    UpdateOnDraw = () => $"{Math.Clamp(Fps.Invoke(), 0, MaxFps):0}",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreRight
                                },
                                new TextDrawable
                                {
                                    Text = $" / {MaxFps} fps",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreLeft
                                },
                            ]
                        },
                        new FrameUpdatableTextDrawable
                        {
                            UpdateOnDraw = () => $"({FrameTime.Invoke()}ms)",
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight
                        },
                    ]
                }
            ];
        }
    }
}