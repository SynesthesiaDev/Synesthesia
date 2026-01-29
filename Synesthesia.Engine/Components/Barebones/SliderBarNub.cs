// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Components.Barebones;

public abstract class SliderBarNub(BarebonesSliderBar owningSliderBar) : CompositeDrawable2d
{
    public readonly BarebonesSliderBar OwningSliderBar = owningSliderBar;
    private Vector2 lastMousePos = Vector2.Zero;

    public bool IsDragged { get; private set; }

    protected internal override bool OnMouseDown(PointInput e)
    {
        IsDragged = true;
        return true;
    }

    protected internal override void OnMouseUp(PointInput e)
    {
        IsDragged = false;
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        base.OnUpdate(frameInfo);
        if (IsDragged)
        {
            var mousePos = InputManager.MousePosition;
            if (lastMousePos != mousePos)
            {
                lastMousePos = mousePos;
                OwningSliderBar.UpdateFromPositionalInput(mousePos);
            }
        }
    }
}
