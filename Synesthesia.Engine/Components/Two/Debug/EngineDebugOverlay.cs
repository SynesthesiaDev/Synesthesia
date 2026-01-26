using System.Numerics;
using Common.Logger;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Components.Two.Debug;

public class EngineDebugOverlay : CompositeDrawable2d
{
    public static readonly ActionBinding ENGINE_DEBUG_OVERLAY_TOGGLE = new()
    {
        Keyboard = new KeyboardBinding(KeyboardKey.F1, KeyboardKey.LeftControl),
        ActionName = "Toggle Engine Debug Overlay",
    };

    // public static readonly HotKey EngineDebugOverlayToggle = new(KeyboardKey.F1, KeyboardKey.LeftControl);
    //
    // public static readonly HotKey FrameCounterToggle = new(KeyboardKey.F2, KeyboardKey.LeftControl);
    // public static readonly HotKey EngineStatisticsToggle = new(KeyboardKey.F3, KeyboardKey.LeftControl);
    // public static readonly HotKey LoggerOverlayToggle = new(KeyboardKey.F4, KeyboardKey.LeftControl);

    private FrameCounter frameCounter = null!;
    private EngineStatisticsPanel engineStatisticsPanel = null!;
    private DebugLoggerOverlay debugLoggerOverlay = null!;

    protected internal override void OnUpdate()
    {
        if (!Visible) return;
        base.OnUpdate();
    }

    protected override void OnLoading()
    {
        Children =
        [
            new FillFlowContainer2d
            {
                Position = new Vector2(10, 10),
                AutoSizeAxes = Axes.Both,
                Direction = Direction.Vertical,
                Spacing = 10f,
                Children =
                [
                    frameCounter = new FrameCounter
                    {
                        Visible = EngineConfiguration.ShowFps,
                        Scale = new Vector2(0.8f)
                    },
                    new AudioDebugOverlay()
                    {
                        Scale = new Vector2(0.8f)
                    },
                    engineStatisticsPanel = new EngineStatisticsPanel
                    {
                        Visible = EngineConfiguration.ShowEngineStatistics,
                        Scale = new Vector2(0.8f)
                    },
                ]
            },

            debugLoggerOverlay = new DebugLoggerOverlay
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Position = new Vector2(10, -10),
                Visible = EngineConfiguration.ShowLogOverlay,
                Scale = new Vector2(0.8f)
            },
        ];

        InputManager.RegisterActionInput(ENGINE_DEBUG_OVERLAY_TOGGLE);

        base.OnLoading();
    }

    protected internal override bool OnActionBindingDown(ActionBinding e)
    {
        if (e != ENGINE_DEBUG_OVERLAY_TOGGLE) return base.OnActionBindingDown(e);

        Visible = !Visible;
        return true;
    }

    private static void toggle(Drawable2d drawable)
    {
        drawable.Visible = !drawable.Visible;
        var name = drawable.GetType()
            .Name;
        Logger.Verbose($"Toggled visibility of {name} to {drawable.Visible}");
    }
}
