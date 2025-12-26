using System.Numerics;
using Raylib_cs;

namespace Synesthesia.Engine.Graphics.Three;

public abstract class Drawable3d : Drawable
{
    public Vector3 Position { get; set; } = new(0, 0, 0);

    public Vector3 Size { get; set; } = new(1);

    public Drawable3d? Parent { get; set; }

    public Matrix4x4 LocalMatrix
    {
        get
        {
            const float degToRad = MathF.PI / 180f;
            var rotation =
                Matrix4x4.CreateRotationX(Rotation.X * degToRad) *
                Matrix4x4.CreateRotationY(Rotation.Y * degToRad) *
                Matrix4x4.CreateRotationZ(Rotation.Z * degToRad);

            return
                Matrix4x4.CreateScale(Size) *
                rotation *
                Matrix4x4.CreateTranslation(Position);
        }
    }

    public Matrix4x4 WorldMatrix
    {
        get
        {
            if (Parent is null) return LocalMatrix;
            return Parent.WorldMatrix * LocalMatrix;
        }
    }

    protected internal sealed override void OnDraw()
    {
        if (!Visible) return;

        BeginLocalSpace();
        try
        {
            OnDraw3d();
        }
        finally
        {
            EndLocalSpace();
        }
    }

    protected abstract void OnDraw3d();

    private void BeginLocalSpace()
    {
        Rlgl.PushMatrix();

        Rlgl.Translatef(Position.X, Position.Y, Position.Z);

        if (Rotation.X != 0) Rlgl.Rotatef(Rotation.X, 1f, 0f, 0f);
        if (Rotation.Y != 0) Rlgl.Rotatef(Rotation.Y, 0f, 1f, 0f);
        if (Rotation.Z != 0) Rlgl.Rotatef(Rotation.Z, 0f, 0f, 1f);

        Rlgl.Scalef(Size.X, Size.Y, Size.Z);

        //TODO shear
    }

    private void EndLocalSpace()
    {
        Rlgl.PopMatrix();
    }
}