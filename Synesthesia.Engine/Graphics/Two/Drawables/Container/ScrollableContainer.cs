// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Bindable;
using Common.Util;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Container;

public class ScrollableContainer : Container2d
{
    private const int scrollbar_container_width = 10;
    private const float layout_buffer = 10f;

    public Direction ScrollDirection { get; set; } = Direction.Vertical;

    public readonly float ScrollDistance = 80;

    private readonly BindableDouble currentScrollPosition = new();

    public double ScrollPosition
    {
        get => currentScrollPosition.Value;
        set => ScrollTo(value);
    }

    public IEnumerable<Drawable2d> ScrollContent
    {
        get => scrollableContainer.Children;
        set => scrollableContainer.Children = value.ToList();
    }

    private bool contentExtendsContainer = false;

    public bool ContentExtendsContainer
    {
        get => contentExtendsContainer;
        set
        {
            if (contentExtendsContainer == value) return;
            contentExtendsContainer = value;
            updateScrollBarState();
        }
    }

    private Container2d viewport { get; } = new BackgroundContainer2d
    {
        RelativeSizeAxes = Axes.Both,
    };

    private Container2d scrollableContainer { get; } = new()
    {
        RelativeSizeAxes = Axes.Both,
    };

    private BackgroundContainer2d scrollbarContainer = null!;

    private void updateScrollBarState()
    {
        if (!ContentExtendsContainer)
        {
            scrollbarContainer.ResizeWidthTo(0f, 200, Easing.InCubic).ThenHide(scrollbarContainer);
        }
        else
        {
            scrollbarContainer.Visible = true;
            scrollbarContainer.ResizeWidthTo(scrollbar_container_width, 200, Easing.OutCubic);
        }
    }

    protected override void OnLoading()
    {
        Masking = true;
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
            scrollbarContainer = new BackgroundContainer2d
            {
                RelativeSizeAxes = Axes.Y,
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Width = 0,
                Visible = false,
                BackgroundColor = Defaults.BACKGROUND3,
            }
        ];
    }

    private double extent
    {
        get
        {
            var contentSize = scrollableContainer.GetChildrenSize();
            return ScrollDirection == Direction.Vertical
                ? Math.Max(0.0, contentSize.Y - Size.Y)
                : Math.Max(0.0, (contentSize.X + layout_buffer) - Size.X);
        }
    }

    protected override void LoadComplete()
    {
        currentScrollPosition.OnValueChange(e =>
        {
            var scrollValue = extent <= 0.0 ? 0.0 : Math.Clamp(e.NewValue, 0.0, extent);

            var newPosition = ScrollDirection == Direction.Vertical
                ? new Vector2(0f, -(float)scrollValue)
                : new Vector2(-(float)scrollValue, 0f);
        });
    }

    private Vector2 lastChildrenSize = Vector2.Zero;

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        var currentSize = scrollableContainer.GetChildrenSize();
        if (lastChildrenSize != currentSize)
        {
            lastChildrenSize = currentSize;
            if (currentScrollPosition.Value > MaxScrollPosition)
                currentScrollPosition.Value = MaxScrollPosition;
        }

        var targetPos = ScrollDirection == Direction.Vertical
            ? new Vector2(0f, -(float)currentScrollPosition.Value)
            : new Vector2(-(float)currentScrollPosition.Value, 0f);

        if (Vector2.Distance(scrollableContainer.Position, targetPos) > 0.1f)
        {
            scrollableContainer.Position = Transforms.VECTOR2.GetValueAt(
                frameInfo.Delta.ToFloat(),
                scrollableContainer.Position,
                targetPos,
                0,
                currentAnimationLength,
                Easing.OutCubic
            );
        }
        else
        {
            scrollableContainer.Position = targetPos;
        }

        base.OnUpdate(frameInfo);
    }

    public void ResetScrollPosition() => ScrollTo(0.0);

    public void ScrollBy(double amount, int animationLenght = 350) => ScrollTo(currentScrollPosition.Value + amount, animationLenght);

    public double MaxScrollPosition => extent;

    private int currentAnimationLength = 350;

    public void RemoveScrollChild(Drawable2d child)
    {
        scrollableContainer.RemoveChild(child);
    }

    public void ScrollTo(double amount, int animationLength = 350)
    {
        currentAnimationLength = animationLength;
        currentScrollPosition.Value = Math.Clamp(amount, 0.0, extent);
    }

    protected internal override bool OnMouseWheel(float delta)
    {
        if (Math.Abs(delta) > 0.0001f)
        {
            ScrollBy(-delta * ScrollDistance);
        }

        return true;
    }

    protected override void Dispose(bool isDisposing)
    {
        currentScrollPosition.Dispose();
        base.Dispose(isDisposing);
    }
}
