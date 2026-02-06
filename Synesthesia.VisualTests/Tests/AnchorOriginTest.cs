using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.VisualTests.Tests;

public class AnchorOriginTest : VisualTest
{
    private Container2d gridRoot = null!;

    private readonly List<(Container2d Cell, DrawableBox2d Marker, Anchor Anchor, Anchor Origin)> cases = new();

    private const float cell_w = 220f;
    private const float cell_h = 160f;

    private static bool near(float a, float b, float eps = 0.75f) => MathF.Abs(a - b) <= eps;

    protected override void OnLoading()
    {
        Scale = new Vector2(0.5f);
        Children =
        [
            gridRoot = new Container2d
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Vertical,
                        Spacing = 14f,
                        Children =
                        [
                            makeRow(Anchor.TopLeft,     Anchor.TopCentre,     Anchor.TopRight),
                            makeRow(Anchor.CentreLeft,  Anchor.Centre,        Anchor.CentreRight),
                            makeRow(Anchor.BottomLeft,  Anchor.BottomCentre,  Anchor.BottomRight),
                        ]
                    }
                ]
            }
        ];

        AddStep("Reset scale", () => { gridRoot.Scale = Vector2.One; }, runNextImmediately: true);

        AddAssert("Validate all anchors (Origin=Centre)", () => validateAll(eps: 0.75f),
            "Each marker should sit at the correct anchor point of its cell, accounting for origin.");

        AddStep("Scale grid root to 1.35", () => { gridRoot.Scale = new Vector2(1.35f); }, runNextImmediately: true);

        AddAssert("Validate after scaling", () => validateAll(eps: 1.25f),
            "Scaled parents tend to reveal double-scaling or wrong origin handling.");

        base.OnLoading();
    }

    private Drawable2d makeRow(Anchor a0, Anchor a1, Anchor a2)
    {
        return new FillFlowContainer2d
        {
            AutoSizeAxes = Axes.Both,
            Direction = Direction.Horizontal,
            Spacing = 14f,
            Children =
            [
                makeCellCase(a0, origin: Anchor.Centre),
                makeCellCase(a1, origin: Anchor.Centre),
                makeCellCase(a2, origin: Anchor.Centre),
            ]
        };
    }

    private Drawable2d makeCellCase(Anchor anchor, Anchor origin)
    {
        var cell = new Container2d
        {
            Size = new Vector2(cell_w, cell_h),
        };

        var background = new DrawableBox2d
        {
            RelativeSizeAxes = Axes.Both,
            Color = new Color(35, 35, 35, 255),
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
        };

        var marker = new DrawableBox2d
        {
            Size = new Vector2(18, 18),
            Color = Color.Gold,
            Anchor = anchor,
            Origin = origin,
            Position = Vector2.Zero,
        };

        var centerDot = new DrawableBox2d
        {
            Size = new Vector2(6, 6),
            Color = new Color(160, 160, 160, 255),
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        };

        cell.Children = [background, centerDot, marker];

        cases.Add((cell, marker, anchor, origin));
        return cell;
    }

    private bool validateAll(float eps)
    {
        foreach (var (cell, marker, anchor, origin) in cases)
        {
            var expected = expectedScreenSpacePosition(
                parentScreenPos: cell.ScreenSpacePosition,
                parentSize: cell.Size,
                parentScale: cell.InheritedScale,
                childSize: marker.Size,
                childScale: marker.InheritedScale,
                childAnchor: anchor,
                childOrigin: origin,
                childPosition: marker.Position
            );

            var actual = marker.ScreenSpacePosition;

            if (!near(actual.X, expected.X, eps) || !near(actual.Y, expected.Y, eps))
                return false;
        }

        return true;
    }

    private static Vector2 expectedScreenSpacePosition(
        Vector2 parentScreenPos,
        Vector2 parentSize,
        Vector2 parentScale,
        Vector2 childSize,
        Vector2 childScale,
        Anchor childAnchor,
        Anchor childOrigin,
        Vector2 childPosition)
    {
        // parentScreenPos + anchorOffset(parentSize, childAnchor) * parentScale
        // + childPosition * parentScale
        // - originOffset(childSize, childOrigin) * childScale
        var anchorOffset = anchorToOffset(parentSize, childAnchor) * parentScale;
        var originOffset = anchorToOffset(childSize, childOrigin) * childScale;
        var posOffset = childPosition * parentScale;

        return parentScreenPos + anchorOffset + posOffset - originOffset;
    }

    private static Vector2 anchorToOffset(Vector2 size, Anchor anchor)
    {
        return anchor switch
        {
            Anchor.TopLeft => new Vector2(0, 0),
            Anchor.TopCentre => new Vector2(size.X / 2f, 0),
            Anchor.TopRight => new Vector2(size.X, 0),

            Anchor.CentreLeft => new Vector2(0, size.Y / 2f),
            Anchor.Centre => new Vector2(size.X / 2f, size.Y / 2f),
            Anchor.CentreRight => new Vector2(size.X, size.Y / 2f),

            Anchor.BottomLeft => new Vector2(0, size.Y),
            Anchor.BottomCentre => new Vector2(size.X / 2f, size.Y),
            Anchor.BottomRight => new Vector2(size.X, size.Y),

            _ => Vector2.Zero
        };
    }
}
