using Synesthesia.Engine;
using Synesthesia.Engine.Util.Bindables;

namespace Synesthesia.Demo;

internal static class Demo
{
    [STAThread]
    private static void Main(string[] args)
    {
        var game = new GameBuilder().Build();
        var toggled = new Bindable<bool>(false);

        game.OnInitialized.Subscribe(_ =>
        {
            game.DrawableScene2D.Children =
            [
            ];
        });

        game.Run();
    }
}
