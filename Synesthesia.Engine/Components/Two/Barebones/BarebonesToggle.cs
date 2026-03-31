// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Util.Bindables;

namespace Synesthesia.Engine.Components.Two.Barebones;

public abstract class BarebonesToggle : CompositeDrawable2d
{
    private BindableListener<bool> checkedListener = null!;

    [ExternalOwnership]
    public required Bindable<bool> Checked { get; set; }

    protected abstract void OnToggle(bool toggled);

    protected override void LoadComplete()
    {
        checkedListener = Checked.OnValueChange(e => OnToggle(e.NewValue), true);
        base.LoadComplete();
    }

    protected internal override void OnMouseUp(ICursorInputEvent e)
    {
        if(!Contains(InputHandler.MousePosition)) return;
        Checked.Value = !Checked.Value;
    }

    protected internal override bool OnMouseDown(ICursorInputEvent e)
    {
        Logger.Verbose("AAAAAaa");
        return true;
    }

    // protected internal override bool OnHover(IPositionalInputEvent e)
    // {
        // return true;
    // }

    protected override void Dispose(bool isDisposing)
    {
        Checked.Unregister(checkedListener);
        base.Dispose(isDisposing);
    }
}
