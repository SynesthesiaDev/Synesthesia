// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util;
using Synesthesia.Engine.Util.Bindables;

namespace Synesthesia.Engine.Components.Two.Debug;

public class FrameCounter : EngineDebugElement
{
    [Singleton]
    private Game game = null!;

    protected override void OnLoading()
    {
        Size = new Vector2(310, 124);
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
                            new HeaderComponent("Frame Counter (Ctrl + F1)"),
                            new PerformanceMonitorElement(game.RenderThread),
                            new PerformanceMonitorElement(game.UpdateThread),
                            new PerformanceMonitorElement(game.InputThread),
                            new PerformanceMonitorElement(game.AudioThread)
                        ],
                        Direction = Direction.Vertical
                    }
                ]
            }
        ];
    }

    private class PerformanceMonitorElement(ThreadRunner thread) : CompositeDrawable2d
    {
        private double lastFps;
        private double lastFrameTime;

        private long maxFps = thread.ActiveUpdateRate.Value;

        private Text2d fpsText = null!;
        private Text2d frameTimeText = null!;
        private Text2d maxFpsText = null!;

        private BindableListener<bool> activeRateListener = null!;
        private ThrottledUpdater throttledUpdater = new(100);

        protected override void OnLoading()
        {
            Size = new Vector2(270, 16);

            Children =
            [
                new Container2d
                {
                    RelativeSizeAxes = Axes.Both,
                    Children =
                    [
                        new Text2d
                        {
                            Text = $"{thread.Type}:",
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Color = EngineBranding.TEXT1
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
                                maxFpsText = new Text2d
                                {
                                    Text = $" / {maxFps} fps",
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

        protected override void LoadComplete()
        {
            activeRateListener = thread.IsActive.OnValueChange(e =>
            {
                maxFps = e.NewValue ? thread.ActiveUpdateRate.Value : thread.InactiveUpdateRate.Value;
                maxFpsText.Text = $" / {maxFps} fps";
            });
        }


        protected internal override void OnUpdate(FrameInfo frameInfo)
        {
            if (throttledUpdater.TryUpdate(frameInfo.Delta))
            {
                var currentFps = thread.Fps;
                var currentFrameTime = thread.FrameTime;

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

        protected override void Dispose(bool isDisposing)
        {
            thread.IsActive.Unregister(activeRateListener);
            base.Dispose(isDisposing);
        }
    }
}
