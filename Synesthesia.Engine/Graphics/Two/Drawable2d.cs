using System.Numerics;
using Common.Logger;
using Common.Util;
using Raylib_cs;

namespace Synesthesia.Engine.Graphics.Two;

public abstract class Drawable2d : Drawable
{
    public Vector2 Position { get; set; } = new(0, 0);

    public Anchor Anchor { get; set; } = Anchor.TopLeft;

    public Anchor Origin { get; set; } = Anchor.TopLeft;

    public Axes AutoSizeAxes { get; set; } = Axes.None;

    public Axes RelativeSizeAxes { get; set; } = Axes.None;

    public Vector2 Size { get; set; } = new(1);

    public Vector2 Scale { get; set; } = new(1);

    public Vector4 Margin { get; set; } = new(0);

    public Drawable2d? Parent { get; set; }

    public long Depth = 0;

    protected internal override void OnUpdate()
    {
        if (RelativeSizeAxes != Axes.None && AutoSizeAxes != Axes.None)
        {
            throw new InvalidOperationException("Cannot have both 'AutoSizeAxis' and 'RelativeSizeAxes'");
        }

        if (Parent != null)
        {
            Size = RelativeSizeAxes switch
            {
                Axes.X => Size with { X = Parent.Size.X - Margin.X - Margin.Z },
                Axes.Y => Size with { Y = Parent.Size.Y - Margin.Y - Margin.W },
                Axes.Both => Size with { X = Parent.Size.X - Margin.X - Margin.Z } with { Y = Parent.Size.Y - Margin.Y - Margin.W },
                _ => Size
            };
        }
    }

    protected internal sealed override void OnDraw()
    {
        if (!Visible) return;

        BeginLocalSpace();

        try
        {
            OnDraw2d();
        }
        finally
        {
            EndLocalSpace();
        }
    }

    private void updateLayout()
    {

    }

    protected abstract void OnDraw2d();

    private void BeginLocalSpace()
    {
        Rlgl.PushMatrix();

        var anchorPos = Vector2.Zero;
        if (Parent != null)
        {
            anchorPos = GetAnchorOffset(Parent.Size, Anchor);
        }

        var originOffset = GetAnchorOffset(Size, Origin);

        Rlgl.Translatef(anchorPos.X + Position.X + Margin.X, anchorPos.Y + Position.Y + Margin.Y, 0);

        if (Rotation.Z != 0) Rlgl.Rotatef(Rotation.Z, 0, 0, 1);

        Rlgl.Translatef(-originOffset.X, -originOffset.Y, 0);

        Rlgl.Scalef(Scale.X, Scale.Y, 1);
    }

    private void EndLocalSpace()
    {
        Rlgl.PopMatrix();
    }

    private static Vector2 GetAnchorOffset(Vector2 size, Anchor anchor)
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