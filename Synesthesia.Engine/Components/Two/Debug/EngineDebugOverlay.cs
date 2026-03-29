// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.ActionBindings;
using Synesthesia.Engine.Timing;
using SynesthesiaUtil;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Components.Two.Debug;

public class EngineDebugOverlay : CompositeDrawable2d
{
    private const long toggle_animation_length = 150;
    private const Easing toggle_animation_easing = Easing.Linear;

    private readonly PlatformActionBinding toggleFrameCounter = new ActionBindingBuilder()
        .AddKeyboard(Key.F1, Key.LControl)
        .Build();

    private readonly PlatformActionBinding toggleStatisticsPanel = new ActionBindingBuilder()
        .AddKeyboard(Key.F2, Key.LControl)
        .Build();

    private Dictionary<PlatformActionBinding, EngineDebugElement> engineDebugElements = [];

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if (!Visible) return;
        base.OnUpdate(frameInfo);
    }

    private FrameCounter frameCounter = null!;
    private StatisticsPanel statisticsPanel = null!;

    protected override void OnLoading()
    {
        Visible = true;
        Children =
        [
            new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Children =
                [
                    new FillFlowContainer2d
                    {
                        Position = new Vector2(10, 10),
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Vertical,
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Spacing = 10f,
                        Children =
                        [
                            frameCounter = new FrameCounter
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                            },
                            statisticsPanel = new StatisticsPanel
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                            }
                        ]
                    },
                ]
            },
        ];

        base.OnLoading();
    }

    protected override void LoadComplete()
    {
        engineDebugElements = Maps.Of<PlatformActionBinding, EngineDebugElement>
        (
            (toggleFrameCounter, frameCounter),
            (toggleStatisticsPanel, statisticsPanel)
        );

        foreach (var (binding, _) in engineDebugElements)
        {
            binding.Register();
        }
    }

    protected internal override bool OnPlatformBindingDown(PlatformActionBinding e)
    {
        var drawable = engineDebugElements.GetOrNull(e);
        if (drawable == null) return false;

        if (drawable.Visible)
        {
            drawable.FadeTo(0f, toggle_animation_length, toggle_animation_easing);
            drawable.ScaleTo(new Vector2(1f, 0f), toggle_animation_length, toggle_animation_easing).ThenHide(drawable);
        }
        else
        {
            drawable.Visible = true;
            drawable.FadeTo(1f, toggle_animation_length, toggle_animation_easing);
            drawable.ScaleTo(new Vector2(1f), toggle_animation_length, toggle_animation_easing);
        }

        return true;
    }
}
