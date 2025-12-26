using System.Numerics;
using Common.Util;
using Raylib_cs;
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
        if(!Visible) return;
        base.OnUpdate();
    }

    protected override void OnLoading()
    {
        Children =
        [
            new FillFlowContainer2d
            {
                Position = new Vector2(10, 10),
                Direction = Direction.Vertical,
                Spacing = 10f,
                Children =
                [
                    _frameCounter = new FrameCounter(),
                    _engineStatisticsPanel = new EngineStatisticsPanel(),
                ]
            },

            _debugLoggerOverlay = new DebugLoggerOverlay
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Position = new Vector2(10, -10),
            }
        ];

        InputManager.Register(FrameCounterToggle);
        InputManager.Register(EngineStatisticsToggle);
        InputManager.Register(EngineDebugOverlayToggle);
        InputManager.Register(LoggerOverlayToggle);

        InputManager.Press.Subscribe(e =>
        {
            if (e.HotKey == EngineDebugOverlayToggle) Visible = !Visible;

            if (!Visible) return;

            if (e.HotKey == FrameCounterToggle) _frameCounter.Visible = !_frameCounter.Visible;

            if (e.HotKey == EngineStatisticsToggle) _engineStatisticsPanel.Visible = !_engineStatisticsPanel.Visible;

            if (e.HotKey == LoggerOverlayToggle) _debugLoggerOverlay.Visible = !_debugLoggerOverlay.Visible;
        });
        base.OnLoading();
    }
}