// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common;
using Common.Logger;
using Common.Pooling;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Utility;

namespace Synesthesia.VisualTests;

public class StepButton : CompositeDrawable2d, IPooledObject
{
    public Action? Action { get; set; }

    private string name = string.Empty;
    public required string Name
    {
        get => name;
        set
        {
            name = value;
            if (Text != null) Text.Text = value;
        }
    }

    protected Color RunningColor => Defaults.YELLOW;

    protected Color IdleColor = Defaults.ACCENT;

    protected FillFlowContainer2d BackgroundContainer = null!;

    protected DrawableBox2d Highlight = null!;

    protected TextDrawable? Text = null;

    public bool RunNextStepImmediately = false;

    protected CompletableFuture<bool> Future = new();

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }

    protected override bool OnHover(MouseMoveInputEvent e)
    {
        BackgroundContainer.FadeBackgroundTo(Defaults.BACKGROUND2.ChangeBrightness(0.1f), 100, Easing.InCubic);
        return true;
    }

    protected override void OnHoverLost(MouseMoveInputEvent e)
    {
        BackgroundContainer.FadeBackgroundTo(Defaults.BACKGROUND2, 100, Easing.OutCubic);
    }

    protected override bool OnMouseDown(PointInput e)
    {
        BackgroundContainer.ScaleTo(0.9f, 2000, Easing.OutQuint);
        return true;
    }

    protected override void OnMouseUp(PointInput e)
    {
        BackgroundContainer.ScaleTo(1f, 1000, Easing.OutElastic);
        if (Contains(e.MousePosition)) PerformStep(true);
    }

    protected override void OnLoading()
    {
        Height = 24;
        RelativeSizeAxes = Axes.X;

        Children =
        [
            BackgroundContainer = new FillFlowContainer2d
            {
                RelativeSizeAxes = Axes.Both,
                Direction = Direction.Horizontal,
                BackgroundColor = Defaults.BACKGROUND2,
                Spacing = 4,
                Children =
                [
                    Highlight = new DrawableBox2d
                    {
                        Color = IdleColor,
                        RelativeSizeAxes = Axes.Y,
                        Width = 5,
                    },
                    Text = new TextDrawable
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
        BackgroundContainer.FlashBackground(Defaults.BACKGROUND3, 100, 300, Easing.In, Easing.Out);
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
        Highlight.FadeColorTo(Defaults.RED, 200, Easing.OutQuad);
        BackgroundContainer.FadeBackgroundTo(Defaults.BACKGROUND2_FAILED, 300, Easing.OutCubic);
        Future.Complete(false);
    }

    protected virtual void Success()
    {
        Highlight.FadeColorTo(Defaults.GREEN, 200, Easing.OutQuad);
        Future.Complete(true);
    }

    public virtual void Reset()
    {
        Text.Text = string.Empty;
        Action = null;
        Future = new CompletableFuture<bool>();
        BackgroundContainer.BackgroundColor = Defaults.BACKGROUND2;
        Highlight.Color = IdleColor;
    }

}
