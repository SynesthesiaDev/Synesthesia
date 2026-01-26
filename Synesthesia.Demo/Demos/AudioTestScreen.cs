// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Bindable;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine;
using Synesthesia.Engine.Audio;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using Synesthesia.Engine.Resources;

namespace Synesthesia.Demo.Demos;

public class AudioTestScreen : Screen
{
    private DefaultEngineButton togglePlayButton = null!;
    private TextDrawable text = null!;

    private readonly AudioSample music = ResourceManager.Get<AudioSample>("SynesthesiaResources.audio.mp3");
    private AudioMixer mixer = null!;

    private readonly Bindable<bool> audioPlaying = new(false);
    private AudioSampleInstance? audioSampleInstance = null;

    protected override void OnLoading()
    {
        Children =
        [
            new FillFlowContainer2d
            {
                AutoSizeAxes = Axes.Both,
                Direction = Direction.Vertical,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = 10f,
                Children =
                [
                    new FillFlowContainer2d
                    {
                        Direction = Direction.Vertical,
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Children =
                        [
                            text = new TextDrawable
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            }
                        ]
                    },
                    new FillFlowContainer2d
                    {
                        Direction = Direction.Horizontal,
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,

                        Children =
                        [
                            togglePlayButton = new DefaultEngineButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(120, 40),
                                Text = "Play",
                                TextColor = Color.Black,
                                ColorCombination = DefaultEngineColorCombination.ACCENT,
                                OnClick = () =>
                                {
                                    if (audioSampleInstance == null)
                                    {
                                        mixer.Volume = 0.5f;
                                        audioSampleInstance = mixer.Play(music);
                                        audioSampleInstance.Pause();
                                        audioPlaying.Value = true;
                                    }
                                    else
                                    {
                                        audioPlaying.Value = !audioPlaying.Value;
                                    }
                                }
                            },
                        ]
                    }
                ]
            }
        ];
    }

    protected override void LoadComplete()
    {
        var game = DependencyContainer.Get<Game>();
        mixer = game.MasterAudioMixer;

        text.Text = $"music data: {music.Data.Length} bytes";

        audioPlaying.OnValueChange(e =>
        {
            if (e.NewValue)
            {
                audioSampleInstance?.Resume();
                togglePlayButton.Text = "Pause";
            }
            else
            {
                audioSampleInstance?.Pause();
                togglePlayButton.Text = "Play";
            }
        });

        base.LoadComplete();
    }

    protected override void Dispose(bool isDisposing)
    {
        music.Dispose();
        base.Dispose(isDisposing);
    }
}
