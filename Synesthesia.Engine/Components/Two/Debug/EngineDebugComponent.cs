using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two.Drawables;

namespace Synesthesia.Engine.Components.Two.Debug;

public class EngineDebugComponent : CompositeDrawable2d
{
    private const float not_hovered_opacity = 0.6f;
    
    protected internal override bool OnHover(HoverEvent e)
    {
        FadeTo(1f, 100, Easing.In);
        return true;
    }

    protected internal override void OnHoverLost(HoverEvent e)
    {
        FadeTo(not_hovered_opacity, 100, Easing.Out);
    }

    public EngineDebugComponent()
    {
        Alpha = not_hovered_opacity;
        
    }

}