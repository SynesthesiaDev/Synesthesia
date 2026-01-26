using Synesthesia.Demo.Demos;
using Synesthesia.Engine;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;

namespace Synesthesia.Demo;

public static class Program
{
    public static ScreenStack ScreenStack = null!;

    public static void Main(string[] args)
    {
        var game = new Game
        {
        };

        game.RootComposite2d.OnLoadComplete.Subscribe(_ =>
        {
            game.RootComposite2d.Children =
            [
                ScreenStack = new ScreenStack()
            ];

            ScreenStack.Push(new AudioTestScreen());
        });

        game.Run();
    }
}
