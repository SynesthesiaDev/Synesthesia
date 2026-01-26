// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
using Synesthesia.Engine.Audio;
using Synesthesia.Engine.Audio.Controls;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.Engine.Components.Two.Debug;

public class AudioDebugOverlay : EngineDebugComponent
{
    private AudioManager audioManager = null!;

    private FillFlowContainer2d childrenContainer = null!;

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
                    childrenContainer = new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Direction = Direction.Vertical,
                        Children = []
                    },
                ]
            }
        ];
    }

    protected override void LoadComplete()
    {
        audioManager = DependencyContainer.Get<AudioManager>();

        updateChannelTree();

        base.LoadComplete();
    }

    private void updateChannelTree()
    {
        foreach (var child in childrenContainer.Children)
        {
            childrenContainer.RemoveChild(child);
        }

        var newChildren = new List<Drawable2d> { new NestedContainer("Output", 0, null) };

        foreach (var channel in audioManager.Channels)
        {
            newChildren.Add(new NestedContainer(channel.Name, 1, channel));
            newChildren.AddRange(channel.Mixers.Select(mixer => new NestedContainer(mixer.Identifier, 2, mixer)));
        }

        childrenContainer.Children = newChildren;
    }

    //
    // Output            ||||||||__
    //
    // └ Master          ||||||____
    //   └ master        __________
    //   └ effects       ||||||____
    //
    // └ Music           ||||______
    //   └ foreground    __________
    //   └ background    |_________
    //   └ drums         |||_______
    //

    private class NestedContainer(string name, int levelsDeep, BassDspAudioHandler? dspAudioHandler) : CompositeDrawable2d
    {
        protected override void OnLoading()
        {
            var audioManager = DependencyContainer.Get<AudioManager>();
            Size = new Vector2(330, 24);

            Children =
            [
                new Container2d
                {
                    RelativeSizeAxes = Axes.Both,
                    Children =
                    [
                        new FillFlowContainer2d()
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Direction = Direction.Horizontal,
                            AutoSizeAxes = Axes.Both,
                            Spacing = 4f,
                            Children =
                            [
                                new Container2d
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    Width = levelsDeep * 24f,
                                },
                                new TextDrawable
                                {
                                    Text = $"{name}:",
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                            ]
                        },

                        new DebugAudioMeter(dspAudioHandler)
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                        }
                    ]
                }
            ];
            base.OnLoading();
        }
    }
}
