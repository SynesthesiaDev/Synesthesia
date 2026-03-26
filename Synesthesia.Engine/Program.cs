using Synesthesia.Engine.Components.Two.Debug;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Platform.Host;

namespace Synesthesia.Engine;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var windowHost = new SDL3WindowHost();
        var game = new Game(windowHost);

        game.OnInitialized.Subscribe(_ =>
        {
            game.DrawableScene2d.Children =
            [
                new EngineDebugOverlay
                {
                    RelativeSizeAxes = Axes.Both
                }
                // new Text2d
                // {
                //     Text = "hello there",
                //     Origin = Anchor.Centre,
                //     Anchor = Anchor.Centre
                // }
            ];
        });

        game.Run();
    }
}
