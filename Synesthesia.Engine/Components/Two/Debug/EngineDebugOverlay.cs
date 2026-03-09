using System.Numerics;
using Common.Logger;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Components.Two.Debug;

public class EngineDebugOverlay : CompositeDrawable2d
{
    public static readonly ActionBinding ENGINE_DEBUG_OVERLAY_TOGGLE = new()
    {
        Keyboard = new KeyboardBinding(KeyboardKey.F1, KeyboardKey.LeftControl),
        ActionName = "Toggle Engine Debug Overlay",
    };

    [Resolved]
    private Game game = null!;

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if (!Visible) return;
        base.OnUpdate(frameInfo);
    }

    private Container2d mainContainer = null!;

    private bool petah = false;

    protected override void OnLoading()
    {
        var anchor = game.IsRunningInTestEnvironment ? Anchor.TopRight : Anchor.TopLeft;
        var paddingX = game.IsRunningInTestEnvironment ? -10 : 10;

        Visible = EngineConfiguration.ShowDebugOverlay;
        Children =
        [
            mainContainer = new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = anchor,
                Origin = anchor,
                Children =
                [
                    new FillFlowContainer2d
                    {
                        Position = new Vector2(paddingX, 10),
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Vertical,
                        Anchor = anchor,
                        Origin = anchor,
                        Spacing = 10f,
                        Children =
                        [
                            new FrameCounter
                            {
                                Scale = new Vector2(0.8f),
                                Anchor = anchor,
                                Origin = anchor,
                            },
                            new AudioDebugOverlay
                            {
                                Scale = new Vector2(0.8f),
                                Anchor = anchor,
                                Origin = anchor,
                            },
                            new EngineStatisticsPanel
                            {
                                Scale = new Vector2(0.8f),
                                Anchor = anchor,
                                Origin = anchor,
                            },
                        ]
                    },

                    new DebugLoggerOverlay
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Position = new Vector2(10, -10),
                        Scale = new Vector2(0.8f)
                    },
                ]
            },
        ];

        InputManager.RegisterActionInput(ENGINE_DEBUG_OVERLAY_TOGGLE);

        base.OnLoading();
    }

    protected override void LoadComplete()
    {
        if (Visible && !petah)
        {
            Logger.Debug("petah", Logger.Runtime);
            petah = true;
        }
        base.LoadComplete();
    }

    protected internal override bool OnActionBindingDown(ActionBinding e)
    {
        if (e != ENGINE_DEBUG_OVERLAY_TOGGLE) return base.OnActionBindingDown(e);

        AnimationSequence sequence;
        if (!Visible)
        {
            Visible = true;
            if (!petah)
            {
                Logger.Debug("petah", Logger.Runtime);
                petah = true;
            }
            sequence = new AnimationSequence.Builder()
                .Add(mainContainer.FadeFromTo(0f, 1f, 250, Easing.Out))
                .Add(mainContainer.ScaleFromTo(0.95f, 1f, 250, Easing.Out))
                .Build();
        }
        else
        {
            sequence = new AnimationSequence.Builder()
                .Add(mainContainer.FadeFromTo(1f, 0f, 250, Easing.Out))
                .Add(mainContainer.ScaleFromTo(1f, 0.95f, 250, Easing.Out))
                .Then(() => Visible = false)
                .Build();
        }

        Animator.Value.AddAnimation(sequence);

        return true;
    }
}
