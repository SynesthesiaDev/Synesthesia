using Synesthesia.Engine;

namespace Synesthesia.Editor;

static class Program
{
    static void Main(string[] args)
    {
        var game = new Game();
        game.WindowTitle.Value = "Synesthesia Editor";
        
        
        // game.RootComposite2d.
        game.Run();
    }
}