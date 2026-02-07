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

        var yaw = Rotation.Y * (MathF.PI / 180f);
        var pitch = Rotation.X * (MathF.PI / 180f);

        var forward = Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, worldRot));
        camera.Target = worldPos + forward;

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
