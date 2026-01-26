// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Bindable;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Audio.Controls;
using Synesthesia.Engine.Components.Barebones;
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

    protected internal override void OnUpdate()
    {
        base.OnUpdate();
        if (AudioSource.Value == null) return;

        var peak = AudioSource.Value.Peak;

        audioLeft.Progress.Value = peak.PeakLeft;
        audioRight.Progress.Value = peak.PeakRight;
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

                    new FrameUpdatableTextDrawable
                    {
                        FontSize = 20,
                        Color = Color.White,
                        UpdateOnDraw = () => AudioSource.Value == null ? "0.0db" : $"{MathUtil.LevelToDb(AudioSource.Value!.Peak.Peak):0.0}db"
                    }
                ]
            },

        ];
        AutoSizeAxes = Axes.Both;
    }

    protected override void Dispose(bool isDisposing)
    {
        AudioSource.Dispose();
        base.Dispose(isDisposing);
    }
}
