// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Util;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two.Container;

public class FillFlowContainer2d : Container2d
{
    public Direction Direction
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    } = Direction.Vertical;

    public float Spacing
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    } = 0f;

    protected override void OnLayout(Invalidation dirty)
    {
        base.OnLayout(dirty);

        if (dirty.HasFlagFast(Invalidation.Layout) | dirty.HasFlagFast(Invalidation.Size))
        {
            float currentY = 0;
            float currentX = 0;
            float maxWidth = 0;
            float maxHeight = 0;

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                if (!child.ShouldBeDrawn) continue;

                child.Position = new Vector2(currentX, currentY);

                if (child.FillRemainingAxes.HasFlagFast(Axes.X))
                {
                    var remainingParentX = Math.Max(0f, Size.X - currentX);
                    var sx = child.Scale.X;
                    child.Width = sx == 0 ? 0 : (remainingParentX / sx);
                }

                if (child.FillRemainingAxes.HasFlagFast(Axes.Y))
                {
                    var remainingParentY = Math.Max(0f, Size.Y - currentY);
                    var sy = child.Scale.Y;
                    child.Height = sy == 0 ? 0 : (remainingParentY / sy);
                }


                var childDrawWidth = child.Size.X * child.Scale.X;
                var childDrawHeight = child.Size.Y * child.Scale.Y;

                if (Direction == Direction.Vertical)
                {
                    currentY += childDrawHeight + Spacing;
                    maxWidth = Math.Max(maxWidth, childDrawWidth);
                }
                else
                {
                    currentX += childDrawWidth + Spacing;
                    maxHeight = Math.Max(maxHeight, childDrawHeight);
                }
            }

            if (AutoSizeAxes.HasFlagFast(Axes.X))
            {
                var contentWidth = Direction == Direction.Vertical ? maxWidth : (currentX - Spacing);
                Size = Size with { X = contentWidth + AutoSizePadding.X + AutoSizePadding.Z };
            }

            if (AutoSizeAxes.HasFlagFast(Axes.Y))
            {
                var contentHeight = Direction == Direction.Vertical ? (currentY - Spacing) : maxHeight;
                Size = Size with { Y = contentHeight + AutoSizePadding.Y + AutoSizePadding.W };
            }
        }
    }

    protected override void UpdateAutoSize()
    {
        var maxWidth = 0f;
        var maxHeight = 0f;
        var totalWidth = 0f;
        var totalHeight = 0f;

        for (int i = 0; i < InternalChildren.Count; i++)
        {
            var child = InternalChildren[i];

            if (!child.Visible) continue;

            var childSize = child.Size * child.Scale;

            maxWidth = Math.Max(maxWidth, childSize.X);
            maxHeight = Math.Max(maxHeight, childSize.Y);
            totalWidth += childSize.X + Spacing;
            totalHeight += childSize.Y + Spacing;
        }

        var finalX = Direction == Direction.Vertical ? maxWidth : (totalWidth - Spacing);
        var finalY = Direction == Direction.Vertical ? (totalHeight - Spacing) : maxHeight;

        Size = new Vector2(
            finalX + AutoSizePadding.X + AutoSizePadding.Z,
            finalY + AutoSizePadding.Y + AutoSizePadding.W
        );
    }
}
