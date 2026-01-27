using Synesthesia.Engine;

namespace Synesthesia.VisualTests;

internal static class Program
{
    private static void Main(string[] args)
    {
        var game = new Game();

        game.DeferredActionQueue.Enqueue(() =>
        {
            game.RootComposite2d.Children = [new TestLibrary()];
        });

        game.Run();
    }
}
