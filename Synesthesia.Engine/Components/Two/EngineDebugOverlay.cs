using System.Numerics;
using Common.Logger;
using Common.Statistics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Components.Two;

public class EngineDebugOverlay : CompositeDrawable2d
{
    public static readonly HotKey EngineDebugOverlayToggle = new(KeyboardKey.F1, KeyboardKey.LeftControl);

    public static readonly HotKey FrameCounterToggle = new(KeyboardKey.F2, KeyboardKey.LeftControl);
    public static readonly HotKey EngineStatisticsToggle = new(KeyboardKey.F3, KeyboardKey.LeftControl);
    public static readonly HotKey LoggerOverlayToggle = new(KeyboardKey.F4, KeyboardKey.LeftControl);

    private FrameCounter _frameCounter = null!;
    private EngineStatisticsPanel _engineStatisticsPanel = null!;
    private DebugLoggerOverlay _debugLoggerOverlay = null!;

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
                    _frameCounter = new FrameCounter
                    {
                        Visible = EngineConfiguration.ShowFps,
                    },
                    _engineStatisticsPanel = new EngineStatisticsPanel
                    {
                        Visible = EngineConfiguration.ShowEngineStatistics,
                    },
                ]
            },

            _debugLoggerOverlay = new DebugLoggerOverlay
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Position = new Vector2(10, -10),
                Visible = EngineConfiguration.ShowLogOverlay,
            },

            new EngineDebugSettings
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Position = new Vector2(-10, 10),
            }
        ];

        InputManager.Register(FrameCounterToggle);
        InputManager.Register(EngineStatisticsToggle);
        InputManager.Register(EngineDebugOverlayToggle);
        InputManager.Register(LoggerOverlayToggle);

        InputManager.Press.Subscribe(e =>
        {
            if (e.HotKey == EngineDebugOverlayToggle) toggle(this);

            if (!Visible) return;

            if (e.HotKey == FrameCounterToggle) toggle(_frameCounter);

            if (e.HotKey == EngineStatisticsToggle) toggle(_engineStatisticsPanel);

            if (e.HotKey == LoggerOverlayToggle) toggle(_debugLoggerOverlay);
        });
        base.OnLoading();
    }

    private static void toggle(Drawable2d drawable)
    {
        drawable.Visible = !drawable.Visible;
        var name = drawable.GetType().Name;
        Logger.Verbose($"Toggled visibility of {name} to {drawable.Visible}");
    }
}