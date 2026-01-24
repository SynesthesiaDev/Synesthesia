using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.Engine.Components.Two;

public class DisableableContainer : BackgroundContainer2d, IDisablable
{
    private readonly DrawableBox2d disabledOverlay = new();

    private bool disabled;

    public bool Disabled
    {
        get => disabled;
        set
        {
            disabled = value;
            var newAlpha = value ? 0.5f : 0f;
            if (IsLoaded)
            {
                disabledOverlay.FadeTo(newAlpha, 200, Easing.InExpo);
            }
            else
            {
                disabledOverlay.Alpha = newAlpha;
            }
        }
    }

    protected override void OnLoading()
    {
        disabledOverlay.Load();
        base.OnLoading();
    }

    protected DisableableContainer()
    {
        disabledOverlay.Color = new Color(0, 0, 0, 255);
        disabledOverlay.Alpha = 0f;
    }

    protected override void OnDraw2d()
    {
        base.OnDraw2d();
        DrawDisabledOverlay();
    }

    protected void DrawDisabledOverlay()
    {
        if(!disabledOverlay.IsLoaded) return;
        if (disabledOverlay.Color.A == 0) return;

        disabledOverlay.Size = Size;
        disabledOverlay.Parent = this;

        disabledOverlay.CornerRadius = BackgroundCornerRadius;

        disabledOverlay.OnDraw();
    }

    protected override void Dispose(bool isDisposing)
    {
        disabledOverlay.Dispose();
        base.Dispose(isDisposing);
    }
}