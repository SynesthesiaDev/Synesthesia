// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Logger;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using Synesthesia.Engine.Utility;

namespace Synesthesia.VisualTests;

public class StepButton : CompositeDrawable2d
{
    public Action? Action { get; set; }

    public required string Name { get; set; }

    protected Color RunningColor => Defaults.ACCENT;

    protected Color IdleColor = Defaults.ACCENT;

    protected FillFlowContainer2d BackgroundContainer = null!;

    protected DrawableBox2d Highlight = null!;

    protected TextDrawable Text = null!;

    protected override bool OnHover(HoverEvent e)
    {
        BackgroundContainer.FadeBackgroundTo(Defaults.BACKGROUND2.ChangeBrightness(0.1f), 100, Easing.InCubic);
        return true;
    }

    protected override void OnHoverLost(HoverEvent e)
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
                        FontSize = 22
                    }
                ]
            }
        ];

        base.OnLoading();
    }

    public virtual void Reset()
    {
        Highlight.FadeColorTo(IdleColor, 1000, Easing.OutQuint);
    }

    public virtual void PerformStep(bool userTriggered = false)
    {
        Highlight.FadeColorTo(RunningColor, 400, Easing.OutQuad);
        try
        {
            Action?.Invoke();
            Success();
        }
        catch (Exception exception)
        {
            Failure();
            Logger.Exception(exception, Logger.Runtime);
            throw;
        }
    }

    protected virtual void Failure()
    {
        Highlight.FadeColorTo(Defaults.RED, 200, Easing.OutQuad);
    }

    protected virtual void Success()
    {
        Highlight.FadeColorTo(Defaults.GREEN, 200, Easing.OutQuad);
    }
}
