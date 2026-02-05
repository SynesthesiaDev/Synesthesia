using System.Numerics;
using Raylib_cs;

namespace Synesthesia.Engine.Graphics;

public interface IDrawable
{
    Vector3 Rotation { get; set; }

    Vector3 Shear { get; set; }

    bool Visible { get; set; }

    BlendMode BlendMode { get; set; }

    float Alpha { get; set; }

    void Show()
    {
        Visible = true;
    }

    void Hide()
    {
        Visible = false;
    }
}