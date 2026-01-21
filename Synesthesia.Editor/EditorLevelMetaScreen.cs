using System.Numerics;
using Common;
using Common.Editor;
using Common.Util;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;

namespace Synesthesia.Editor;

public class EditorLevelMetaScreen(Level level) : Screen
{
    public readonly Level Level = level;

    private DefaultEngineTextbox _textbox = null!;

    protected override void OnLoading()
    {
        Children =
        [
            new FillFlowContainer2d()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = Direction.Vertical,
                BackgroundColor = Defaults.Background1,
                BackgroundAlpha = 0.8f,
                AutoSizePadding = new Vector4(20),
                BackgroundCornerRadius = 10,

                Children =
                [
                    new DefaultEngineButton
                    {
                        Size = new Vector2(120, 40),
                        Text = "Back",
                        OnClick = () =>
                        {
                            var editor = DependencyContainer.Get<Editor>();
                            editor.ScreenStack.Pop();
                        }
                    },
                    new DefaultEngineButton
                    {
                        Size = new Vector2(120, 40),
                        Text = "DEEPER!!",
                        OnClick = () =>
                        {
                            var editor = DependencyContainer.Get<Editor>();
                            editor.ScreenStack.Push(new EditorLevelMetaScreen(Level));
                        }
                    },

                    _textbox = new DefaultEngineTextbox
                    {
                        Size = new Vector2(300, 40),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                ],
            },
        ];
    }


    public override CompletableFuture<Nothing> OnExiting(ScreenExitEvent e)
    {
        var future = new CompletableFuture<Nothing>();

        FadeTo(0f, 200, Easing.Out).Then(() => future.Complete());
        return future;
    }

    public override CompletableFuture<Nothing> OnEntering(ScreenTransitionEvent e)
    {
        var future = new CompletableFuture<Nothing>();
        FadeFromTo(0f, 1f, 200, Easing.In).Then(() => future.Complete());
        return future;
    }

    protected override void LoadComplete()
    {
        _textbox.Text.Value = Level.DisplayName;
    }
}