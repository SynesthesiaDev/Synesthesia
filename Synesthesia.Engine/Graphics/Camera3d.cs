// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Raylib_cs;
using Synesthesia.Engine.Graphics.Three;

namespace Synesthesia.Engine.Graphics;

public class Camera3d : Drawable3d
{
    private Camera3D camera;

    public float FoV { get; set; } = 90;

    public Camera3D RaylibCamera => camera;

    public CameraProjection CameraProjection { get; set; } = CameraProjection.Perspective;

    public Vector3 Target { get; set; } = Vector3.Zero;

    public Vector3 Up { get; set; } = Vector3.UnitY;

    protected override void OnDraw3d()
    {
        // no drawing you silly billy
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        var worldPos = WorldPosition;
        var worldRot = WorldRotationQuaternion;

        camera.Position = worldPos;
        camera.FovY = FoV;
        camera.Projection = CameraProjection;

        var forward = Vector3.Transform(Vector3.UnitZ, worldRot);

        var up = Vector3.Transform(Vector3.UnitY, worldRot);

        camera.Target = worldPos + forward;
        camera.Up = up;

        Raylib.UpdateCamera(ref camera, CameraMode.Custom);

        base.OnUpdate(frameInfo);
    }

    protected override void OnLoading()
    {
        camera = new Camera3D
        {
            Position = Position,
            Target = Position + Vector3.UnitZ,
            Up = Vector3.UnitY,
            FovY = FoV,
            Projection = CameraProjection
        };
    }

}
