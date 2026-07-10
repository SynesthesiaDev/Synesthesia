// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Graphics.Two.Text;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Util;
using Synesthesia.Engine.Util.Future;
using Synesthesia.Engine.Util.Pooling;

namespace Synesthesia.VisualTests;

public class StepButton : CompositeDrawable2D, IPooledObject
{
    public Action? Action { get; set; }

    public required string Name
    {
        get;
        set
        {
            field = value;
            Text?.Text = value;
        }
    } = string.Empty;

    protected Color RunningColor => Color.Yellow;

    protected Color IdleColor = EngineBranding.PINK;

    protected FillFlowContainer2D BackgroundContainer = null!;

    protected Box2D Highlight = null!;

    protected Text2D? Text = null;

    public bool RunNextStepImmediately = false;

    protected CompletableFuture<bool> Future = new();

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    protected override bool OnHover(IPositionalInputEvent e)
    {
        BackgroundContainer.FadeBackgroundTo(EngineBranding.BACKGROUND2.Lighten(1.1f), 100, Easing.InCubic);
        return true;
    }

    protected override void OnHoverLost(IPositionalInputEvent e)
    {
        BackgroundContainer.FadeBackgroundTo(EngineBranding.BACKGROUND2, 100, Easing.OutCubic);
    }

    protected override bool OnMouseDown(ICursorInputEvent e)
    {
        BackgroundContainer.ScaleTo(0.9f, 2000, Easing.OutQuint);
        return true;
    }

    protected override void OnMouseUp(ICursorInputEvent e)
    {
        BackgroundContainer.ScaleTo(1f, 1000, Easing.OutElastic);
        if (Contains(InputHandler.MousePosition)) PerformStep(true);
    }

    protected override void OnLoading()
    {
        Height = 24;
        RelativeSizeAxes = Axes.X;

        Children =
        [
            BackgroundContainer = new FillFlowContainer2D
            {
                RelativeSizeAxes = Axes.Both,
                Direction = Direction.Horizontal,
                Spacing = 4,
                Children =
                [
                    Highlight = new Box2D
                    {
                        Color = IdleColor,
                        RelativeSizeAxes = Axes.Y,
                        Width = 5,
                    },
                    Text = new Text2D
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = Name,
                        FontSize = 19
                    }
                ]
            }
        ];

        base.OnLoading();
    }

    public virtual CompletableFuture<bool> PerformStep(bool userTriggered = false)
    {
        Highlight.FadeColorTo(RunningColor, 400, Easing.OutQuad);
        BackgroundContainer.FlashBackground(EngineBranding.BACKGROUND3, 100, 300, Easing.In, Easing.Out);
        try
        {
            Action?.Invoke();
            Success();
        }
        catch (Exception exception)
        {
            Failure();
            Logger.Exception(exception, Logger.Runtime);
        }

        return Future;
    }

    protected virtual void Failure()
    {
        Highlight.FadeColorTo(Color.Red, 200, Easing.OutQuad);
        BackgroundContainer.FadeBackgroundTo(Color.Red, 300, Easing.OutCubic);
        Future.Complete(false);
    }

    protected virtual void Success()
    {
        Highlight.FadeColorTo(Color.Green, 200, Easing.OutQuad);
        Future.Complete(true);
    }

    public virtual void Reset()
    {
        Text.Text = string.Empty;
        Action = null;
        Future = new CompletableFuture<bool>();
        BackgroundContainer.BackgroundColor = EngineBranding.BACKGROUND2;
        Highlight.Color = IdleColor;
    }

}
