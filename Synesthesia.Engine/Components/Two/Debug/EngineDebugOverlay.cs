using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;
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

    protected internal override void OnUpdate()
    {
        if (!Visible) return;
        base.OnUpdate();
    }

    private Container2d mainContainer = null!;

    protected override void OnLoading()
    {
        Visible = EngineConfiguration.ShowDebugOverlay;
        Children =
        [
            mainContainer = new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new FillFlowContainer2d
                    {
                        Position = new Vector2(10, 10),
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Vertical,
                        Spacing = 10f,
                        Children =
                        [
                            new FrameCounter
                            {
                                Scale = new Vector2(0.8f)
                            },
                            new AudioDebugOverlay
                            {
                                Scale = new Vector2(0.8f)
                            },
                            new EngineStatisticsPanel
                            {
                                Scale = new Vector2(0.8f)
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

    protected internal override bool OnActionBindingDown(ActionBinding e)
    {
        if (e != ENGINE_DEBUG_OVERLAY_TOGGLE) return base.OnActionBindingDown(e);

        AnimationSequence sequence;
        if (!Visible)
        {
            Visible = true;
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

        AnimationManager.AddAnimation(sequence);

        return true;
    }
}
