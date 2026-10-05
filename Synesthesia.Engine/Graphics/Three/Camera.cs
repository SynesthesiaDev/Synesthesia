// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using System.Numerics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Utils.Extensions;

namespace Synesthesia.Engine.Graphics.Three;

public class Camera : Drawable3D
{
    public Vector3 Target { get; set; } = Vector3.Zero;

    public Vector3 Up { get; set; } = Vector3.UnitY;

    public Matrix4x4 ViewMatrix { get; private set; } = Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.Zero, Vector3.UnitY);

    protected override void OnLayout(Invalidation dirty)
    {
        base.OnLayout(dirty);
        if (dirty.HasFlagFast(Invalidation.CameraView))
        {
            ViewMatrix = Matrix4x4.CreateLookAt(Position, Target, Up);
        }
    }

    protected override void OnDraw3D()
    {
        // no drawie :c
    }


}
