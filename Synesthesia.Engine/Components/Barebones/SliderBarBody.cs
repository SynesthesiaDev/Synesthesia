// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics.Two.Drawables;

namespace Synesthesia.Engine.Components.Barebones;

public abstract class SliderBarBody(BarebonesSliderBar owningSliderBar) : CompositeDrawable2d
{
    public readonly BarebonesSliderBar OwningSliderBar = owningSliderBar;

    public abstract void ValueChanged(float newValue);

    protected internal override bool OnMouseDown(PointInput e)
    {
        OwningSliderBar.UpdateFromPositionalInput(e.MousePosition);
        return true;
    }

}
