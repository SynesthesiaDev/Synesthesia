// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Raylib_cs;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Container;

public class MaskingContainer2d : Container2d
{
    public bool Masking { get; set; } = true;

    protected override void OnDraw2d()
    {
        if (!Masking)
        {
            base.OnDraw2d();
            return;
        }

        Raylib.BeginScissorMode(
            (int)ScreenSpacePosition.X,
            (int)ScreenSpacePosition.Y,
            Math.Max(0, (int)Size.X),
            Math.Max(0, (int)Size.Y)
        );

        try
        {
            base.OnDraw2d();
        }
        finally
        {
            Raylib.EndScissorMode();
        }
    }
}
