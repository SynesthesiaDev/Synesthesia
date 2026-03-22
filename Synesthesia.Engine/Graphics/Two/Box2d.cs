// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Render;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two;

public class Box2d : Drawable2d
{
    [Resolved]
    private OpenGlRenderer renderer = null!;

    public Color Color = Color.Green;

    private uint packedColor;

    protected override void OnLayout(Invalidation dirty)
    {
        if (dirty.HasFlagFast(Invalidation.DrawNode))
        {
            packedColor = Color.ToRgba32();
            Logger.Verbose("Invalidated color vector", Logger.Render);
        }

        base.OnLayout(dirty);
    }

    protected override void OnDraw2d()
    {
        renderer.DrawQuad(Position, Size, packedColor);
    }
}
