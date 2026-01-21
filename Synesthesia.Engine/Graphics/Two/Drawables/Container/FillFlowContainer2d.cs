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

        foreach (var child in _children.Filter(child => child.Visible))
        {
            child.Position = new Vector2(currentX, currentY);

            child.OnUpdate();

            if (Direction == Direction.Vertical)
            {
                currentY += (child.Size.Y + Spacing) * child.Scale.Y;
                maxWidth = Math.Max(maxWidth, child.Size.X);
            }
            else
            {
                currentX += (child.Size.X + Spacing) * child.Scale.X;
                maxHeight = Math.Max(maxHeight, child.Size.Y);
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

        foreach (var child in _children.Filter(child => child.Visible))
        {
            child.OnDraw();
        }
    }
}