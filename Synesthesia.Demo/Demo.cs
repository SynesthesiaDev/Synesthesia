using Synesthesia.Engine;
using Synesthesia.Engine.Platform.Host;
using Synesthesia.Engine.Util.Bindables;

namespace Synesthesia.Demo;

internal static class Demo
{
    [STAThread]
    private static void Main(string[] args)
    {
        var windowHost = new SDL3WindowHost();
        var game = new Game(windowHost);
        var toggled = new Bindable<bool>(false);

        game.OnInitialized.Subscribe(_ =>
        {
            game.DrawableScene2D.Children = [];
        });

        game.Run();
    }
}
