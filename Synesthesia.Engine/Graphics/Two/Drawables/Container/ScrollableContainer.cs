// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Logger;
using Common.Util;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Container;

public class ScrollableContainer : MaskingContainer2d
{
    public Direction ScrollDirection { get; set; } = Direction.Vertical;

    public bool ScrollbarAlwaysVisible { get; set; } = false;

    public float ScrollDistance = 80;

    public float ClampExtension = 500;

    public double DistanceDecayDrag = 0.0035;

    public double DistanceDecayScroll = 0.01;

    public double DistanceDecayJump = 0.01;

    public double CurrentScrollPosition { get; private set; }

    public IEnumerable<Drawable2d> ScrollContent
    {
        get => scrollableContainer.Children;
        set => scrollableContainer.Children = value.ToList();
    }

    private Container2d viewport { get; } = new BackgroundContainer2d
    {
        RelativeSizeAxes = Axes.Both,
        BackgroundColor = Defaults.BACKGROUND0
    };

    private Container2d scrollableContainer { get; } = new Container2d()
    {
        RelativeSizeAxes = Axes.Both,
    };

    protected override void OnLoading()
    {
        viewport.Children = [scrollableContainer];

        Children =
        [
            new FillFlowContainer2d
            {
                RelativeSizeAxes = Axes.Both,
                Direction = Direction.Horizontal,
                Children =
                [
                    viewport,
                ]
            },
            new BackgroundContainer2d
            {
                RelativeSizeAxes = Axes.Y,
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Width = 10,
                BackgroundColor = Defaults.BACKGROUND3,
            }
        ];
    }

    protected internal override bool OnMouseWheel(float delta)
    {
        Logger.Verbose($"{delta}");
        if (Math.Abs(delta) > 0.0001f)
        {
            CurrentScrollPosition -= delta * ScrollDistance;
        }
        updateScrollOffset();

        return true;
    }

    private void updateScrollOffset()
    {
        var contentSize = scrollableContainer.GetChildrenSize();
        var extent = ScrollDirection == Direction.Vertical
            ? Math.Max(0.0, contentSize.Y - Size.Y)
            : Math.Max(0.0, contentSize.X - Size.X);

        if (extent <= 0.0)
        {
            CurrentScrollPosition = 0.0;
        }
        else
        {
            CurrentScrollPosition = Math.Clamp(CurrentScrollPosition, 0.0, extent);
        }

        var newPosition = ScrollDirection == Direction.Vertical
            ? new Vector2(0f, -(float)CurrentScrollPosition)
            : new Vector2(-(float)CurrentScrollPosition, 0f);

        scrollableContainer.MoveTo(newPosition, 350, Easing.OutQuart);

        Logger.Verbose($"Scroll: {CurrentScrollPosition} ({viewport.Position}");
    }


    // public bool IsScrolledToStart(float lenience = Precision.FLOAT_EPSILON) => Precision.AlmostBigger(0, Target, lenience);

    // public bool IsScrolledToEnd(float lenience = Precision.FLOAT_EPSILON) => Precision.AlmostBigger(Target, ScrollableExtent, lenience);
}
