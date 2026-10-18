// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;

namespace Synesthesia.Engine.Graphics.Three;

public class DrawableScene3D : Drawable3D
{

    private readonly Camera camera = new Camera { Position = new Vector3(0, 2, 5) };
    private readonly Box3D cube = new Box3D { Size = new Vector3(1, 1, 1), Color = Color.White };

    protected override void OnLoading()
    {

    }


    protected override void OnDraw3D()
    {
        Renderer.Camera = camera;
        cube.OnDraw();
    }
}
