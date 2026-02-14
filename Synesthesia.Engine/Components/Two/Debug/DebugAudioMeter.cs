// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Bindable;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Audio.Controls;
using Synesthesia.Engine.Components.Barebones;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Components.Two.Debug;

public class DebugAudioMeter(BassDspAudioHandler? audioHandler = null) : CompositeDrawable2d
{
    public readonly Bindable<BassDspAudioHandler?> AudioSource = new(audioHandler);

    private BarebonesProgressBar audioRight = null!;
    private BarebonesProgressBar audioLeft = null!;

    private double updateTimer;
    private const double interval = 500;

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        base.OnUpdate(frameInfo);
        if (AudioSource.Value == null) return;

        updateTimer += frameInfo.Delta;

        if (updateTimer >= interval)
        {
            var peak = AudioSource.Value.Peak;

            audioLeft.Progress.Value = peak.PeakLeft;
            audioRight.Progress.Value = peak.PeakRight;

            updateTimer -= interval;
        }
    }

    protected override void LoadComplete()
    {
        AudioSource.OnValueChange(e =>
        {
            if (e.NewValue != null) return;

            audioLeft.Progress.Value = 0f;
            audioRight.Progress.Value = 0f;
        });
    }

    protected override void OnLoading()
    {
        Children =
        [
            new FillFlowContainer2d()
            {
                AutoSizeAxes = Axes.Both,
                Spacing = 2f,
                Direction = Direction.Horizontal,
                Children =
                [
                    new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Spacing = 5f,
                        Direction = Direction.Vertical,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Children =
                        [
                            audioLeft = new BarebonesProgressBar
                            {
                                Size = new Vector2(64, 4),
                            },
                            audioRight = new BarebonesProgressBar
                            {
                                Size = new Vector2(64, 4),
                            },
                        ]
                    },

                    new Container2d
                    {
                        Size = new Vector2(80, 20),
                        Children =
                        [
                            new AudioLevelText
                            {
                                FontSize = 20,
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Color = Color.White,
                                Source = AudioSource.Value
                            }
                        ]
                    },
                ]
            },
        ];

        AutoSizeAxes = Axes.Both;
    }

    private class AudioLevelText : TextDrawable
    {
        public BassDspAudioHandler? Source { get; set; }

        private double lastLevel = double.NaN;

        private ThrottledUpdater throttledUpdater = new(750);

        protected internal override void OnUpdate(FrameInfo frameInfo)
        {
            if (throttledUpdater.TryUpdate(frameInfo.Delta))
            {
                if (Source?.Peak.Peak == null)
                {
                    if (Text != "-inf db") Text = "-inf db";
                    return;
                }

                var currentLevel = MathUtil.LevelToDb(Source.Peak.Peak);

                if (!Precision.IsSame(currentLevel, lastLevel, 0.1))
                {
                    lastLevel = currentLevel;
                    Text = currentLevel <= -90 ? "-inf db" : $"{currentLevel:F1} db";
                };
            }

            base.OnUpdate(frameInfo);
        }
    }

    protected override void Dispose(bool isDisposing)
    {
        AudioSource.Dispose();
        base.Dispose(isDisposing);
    }
}
