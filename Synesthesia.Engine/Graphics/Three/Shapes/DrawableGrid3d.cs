// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Raylib_cs;

namespace Synesthesia.Engine.Graphics.Three.Shapes;

public class DrawableGrid3d : Drawable3d
{
    public int Slices { get; set; } = 20;

    public float Spacing { get; set; } = 1f;

    // protected override bool DirectDraw => true;

    protected override void OnDraw3d()
    {
        Raylib.DrawGrid(Slices, Spacing);
    }


}
