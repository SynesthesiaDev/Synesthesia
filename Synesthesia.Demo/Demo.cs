using Synesthesia.Engine;

namespace Synesthesia.Demo;

internal static class Demo
{
    [STAThread]
    private static void Main(string[] args)
    {
        var game = new GameBuilder().Build();

        // game.OnInitialized.Subscribe(_ =>
        // {
        //     game.DrawableScene2D.Children =
        //     [
        //         new SideScrollerTest
        //         {
        //             RelativeSizeAxes = Axes.Both
        //         }
        //     ];
        // });

        game.Run();
    }
}
