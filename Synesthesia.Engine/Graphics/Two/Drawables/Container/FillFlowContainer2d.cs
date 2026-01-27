using System.Numerics;
using Common.Util;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Container;

public class FillFlowContainer2d : BackgroundContainer2d
{
    public Direction Direction { get; set; } = Direction.Horizontal;

    public float Spacing { get; set; } = 0f;

protected internal override void OnUpdate()
    {
        base.OnUpdate();

        float currentY = 0;
        float currentX = 0;
        float maxWidth = 0;
        float maxHeight = 0;

        foreach (var child in InternalChildren.Filter(child => child.Visible))
        {
            child.Position = new Vector2(currentX, currentY);

            if (Direction == Direction.Horizontal && child.FillRemainingAxes.HasFlag(Axes.X))
            {
                var remainingParentX = Math.Max(0f, Size.X - currentX);
                var sx = child.Scale.X;
                child.Width = sx == 0 ? 0 : (remainingParentX / sx);
            }

            if (Direction == Direction.Vertical && child.FillRemainingAxes.HasFlag(Axes.Y))
            {
                var remainingParentY = Math.Max(0f, Size.Y - currentY);
                var sy = child.Scale.Y;
                child.Height = sy == 0 ? 0 : (remainingParentY / sy);
            }

            child.OnUpdate();

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

        if (AutoSizeAxes.HasFlag(Axes.X))
        {
            var contentWidth = Direction == Direction.Vertical ? maxWidth : (currentX - Spacing);
            Size = Size with { X = contentWidth + AutoSizePadding.X + AutoSizePadding.Z };
        }

        if (AutoSizeAxes.HasFlag(Axes.Y))
        {
            var contentHeight = Direction == Direction.Vertical ? (currentY - Spacing) : maxHeight;
            Size = Size with { Y = contentHeight + AutoSizePadding.Y + AutoSizePadding.W };
        }
    }

    protected override void OnDraw2d()
    {
        DrawBackground();

        foreach (var child in InternalChildren.Filter(child => child.Visible))
        {
            child.OnDraw();
        }
    }
}
