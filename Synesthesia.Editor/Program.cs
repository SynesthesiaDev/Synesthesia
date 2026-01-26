using Common.Editor;
using Synesthesia.Engine.Dependency;

namespace Synesthesia.Editor;

static class Program
{
    public static Level CurrentLevel = new Level("Test", "Synesthesia");
    
    static void Main(string[] args)
    {
        var editor = new Editor();
        DependencyContainer.Add(editor);
        
        
        // game.RootComposite2d.
        editor.Run();
    }
}