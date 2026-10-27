using System.Numerics;

namespace Synesthesia.Engine.Graphics.Three;

public class Camera : Drawable3D
{
    private const float fov_min = 10;
    private const float fov_max = 150;

    public Matrix4x4 ViewMatrix { get; private set; } = Matrix4x4.Identity;

    public float Fov
    {
        get;
        set => field = Math.Clamp(value, fov_min, fov_max);
    } = 70f;

    public float ZNear { get; set; } = 0.1f;

    public float ZFar { get; set; } = 1000f;

    protected internal override void OnDraw()
    {
        base.OnDraw();

        if (Matrix4x4.Invert(DrawMatrix.Matrix, out var view))
        {
            ViewMatrix = view;
        }
    }

    protected override void Collect(RenderQueue renderQueue)
    {
        // no drawie :c
    }
}
