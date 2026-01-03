using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

namespace Synesthesia.Engine.Components.Two;

public class DisableableContainer : BackgroundContainer2d
{
    private readonly DrawableBox2d _disabledOverlay = new();

    private bool _disabled;

    public bool Disabled
    {
        get => _disabled;
        set
        {
            _disabled = value;
            var newAlpha = value ? 0.5f : 0f;
            if (IsLoaded)
            {
                _disabledOverlay.FadeAlphaTo(newAlpha, 200, Easing.InExpo);
            }
            else
            {
                _disabledOverlay.Alpha = newAlpha;
            }
        }
    }

    protected override void OnLoading()
    {
        _disabledOverlay.Load();
        base.OnLoading();
    }

    protected DisableableContainer()
    {
        _disabledOverlay.Color = new Color(0, 0, 0, 255);
        _disabledOverlay.Alpha = 0f;
    }

    protected override void OnDraw2d()
    {
        base.OnDraw2d();
        DrawDisabledOverlay();
    }

    protected void DrawDisabledOverlay()
    {
        if(!_disabledOverlay.IsLoaded) return;
        if (_disabledOverlay.Color.A == 0) return;

        _disabledOverlay.Size = Size;
        _disabledOverlay.Parent = this;

        _disabledOverlay.CornerRadius = BackgroundCornerRadius;

        _disabledOverlay.OnDraw();
    }

    protected override void Dispose(bool isDisposing)
    {
        _disabledOverlay.Dispose();
        base.Dispose(isDisposing);
    }
}