// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Util;
using Synesthesia.Engine.Graphics.Two.Drawables;

namespace Synesthesia.VisualTests;

public class VisualTestDrawable(VisualTest test) : CompositeDrawable2d
{
    protected override void OnLoading()
    {
        RelativeSizeAxes = Axes.Both;
        Children = test.Setup();
    }

    protected override void Dispose(bool isDisposing)
    {
        test.Cleanup();
        base.Dispose(isDisposing);
    }
}
