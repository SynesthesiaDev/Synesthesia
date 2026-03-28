// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Platform.Render;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two;

public class Circle2d : Drawable2d
{
    [Singleton]
    private OpenGlRenderer renderer = null!;

    public Color Color
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.DrawNode);
        }
    } = Color.White;

    private uint packedColor;

    protected override void OnLayout(Invalidation dirty)
    {
        if (dirty.HasFlagFast(Invalidation.DrawNode))
        {
            packedColor = Color.ToRgba32();
        }

        base.OnLayout(dirty);
    }

    protected override void OnDraw2d()
    {
        renderer.DrawQuad(
            drawMatrix: DrawMatrix,
            position: Vector2.Zero,
            size: Size,
            packedColor: packedColor,
            alpha: InheritedAlpha,
            borderThickness: BorderThickness,
            borderHasSingleColor: BorderColor.HasSingleColor,
            borderColor: CachedBorderColor,
            cornerRadius: Size.X / 2f
        );
    }
}
