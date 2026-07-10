using Synesthesia.Engine;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two.Text;
using Synesthesia.Engine.Platform.Host;

namespace Synesthesia.VisualTests;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var windowHost = new SDL3WindowHost();
        var game = new Game(windowHost);

        game.OnInitialized.Subscribe(_ =>
        {
            game.DrawableScene2D.Children =
            [
                new Text2D
                {
                    Text = "testing testing",
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre
                }
            ];
        });


        game.Run();
    }
}
