// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Platform.Render;

namespace Synesthesia.Engine.Graphics.Two;

public class Box2d : Drawable2d
{
    [Resolved]
    private OpenGlRenderer renderer = null!;

    protected override void OnDraw2d()
    {
        renderer.Scale(Size.X, Size.Y, 1);
        renderer.QuadRenderer.Draw();
    }
}
