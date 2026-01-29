// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine;
using Synesthesia.Engine.Audio;
using Synesthesia.Engine.Components.Two.Debug;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Resources;

namespace Synesthesia.VisualTests.Tests;

public class SimpleAudioTest : VisualTest
{
    public override string Name => "Audio Test";

    private int currentSampleIndex = 0;
    private AudioSampleInstance currentlyPlayingSample = null!;

    private List<AudioSample> samples = [];

    private AudioManager audioManager = null!;
    private AudioMixer masterAudioMixer = null!;

    public override List<Drawable2d> Setup()
    {
        audioManager = DependencyContainer.Get<AudioManager>();
        masterAudioMixer = DependencyContainer.Get<Game>().MasterAudioMixer;

        samples =
        [
            ResourceManager.Get<AudioSample>("SynesthesiaResources.audio.mp3"),
            ResourceManager.Get<AudioSample>("SynesthesiaResources.audio2.mp3"),
            ResourceManager.Get<AudioSample>("SynesthesiaResources.audio3.mp3"),
            ResourceManager.Get<AudioSample>("SynesthesiaResources.audio4.mp3"),
        ];

        currentSampleIndex = samples.CycleIndex(currentSampleIndex);
        currentlyPlayingSample = masterAudioMixer.Play(samples[currentSampleIndex]);
        currentlyPlayingSample.Pause();

        List<Drawable2d> children =
        [
            new FillFlowContainer2d
            {
                AutoSizeAxes = Axes.Both,
                Direction = Direction.Vertical,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = 10,
                Children =
                [
                    new AudioDebugOverlay
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre
                    },
                    new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Horizontal,
                        Spacing = 10f,
                        Children =
                        [
                            new DefaultButton
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(120, 40),
                                Text = "Cycle track",
                                TextColor = Color.Black,
                                ColorCombination = DefaultEngineColorCombination.ACCENT,
                                OnClick = () =>
                                {
                                    var isPaused = currentlyPlayingSample.IsPaused;

                                    currentlyPlayingSample.Stop();
                                    currentSampleIndex = samples.CycleIndex(currentSampleIndex);
                                    var next = samples[currentSampleIndex];
                                    currentlyPlayingSample = masterAudioMixer.Play(next);
                                    if(isPaused) currentlyPlayingSample.Pause();
                                }
                            },

                            new DefaultButton
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(120, 40),
                                Text = "Play/Pause",
                                TextColor = Color.Black,
                                ColorCombination = DefaultEngineColorCombination.ACCENT,
                                OnClick = () =>
                                {
                                    if (currentlyPlayingSample.IsPaused)
                                    {
                                        currentlyPlayingSample.Resume();
                                    }
                                    else
                                    {
                                        currentlyPlayingSample.Pause();
                                    }
                                }
                            },
                        ]
                    }
                ]
            },
        ];

        return children;
    }


    public override void Cleanup()
    {
        currentlyPlayingSample.Stop();
        samples.Clear();
    }
}
