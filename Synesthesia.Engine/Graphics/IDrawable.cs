using System.Numerics;
using Raylib_cs;

namespace Synesthesia.Engine.Graphics;

public interface IDrawable
{
    public Vector3 Rotation { get; set; }

    public Vector3 Shear { get; set; }

    public bool Visible { get; set; }

    public BlendMode BlendMode { get; set; }

    public float Alpha { get; set; }

    public void Show()
    {
        Visible = true;
    }

    public void Hide()
    {
        Visible = false;
    }
}