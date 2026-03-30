// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Components.Two.Debug;
using Synesthesia.Engine.Graphics.Layout;

namespace Synesthesia.Engine.Graphics.Two.Container;

public class InternalGameContainer2d : CompositeDrawable2d
{
    public DrawableScene2d DrawableScene2d { get; private set; } = null!;

    protected override void OnLoading()
    {
        Children =
        [
            DrawableScene2d = new DrawableScene2d
            {
                RelativeSizeAxes = Axes.Both
            },
            new EngineDebugOverlay
            {
                RelativeSizeAxes = Axes.Both
            },
        ];
    }
}
