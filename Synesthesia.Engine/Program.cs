using Synesthesia.Engine.Platform.Host;

namespace Synesthesia.Engine;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var windowHost = new SDL3WindowHost();
        var game = new Game(windowHost);
        game.Run();
    }
}
