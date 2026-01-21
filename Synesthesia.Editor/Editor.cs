using Common.Editor;
using Synesthesia.Engine;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;

namespace Synesthesia.Editor;

public class Editor : Game
{
    public Level? CurrentLevel = new("Test", "Synesthesia");

    public ScreenStack ScreenStack;

    public void OpenLevel(Level? level)
    {
        if (level == CurrentLevel) return;
        if (level == null) return; //TODO Cleanup

        CurrentLevel = level;
    }

    public Editor()
    {
        WindowTitle.Value = "Synesthesia Editor";
        RootComposite2d.OnLoadComplete.Subscribe(_ =>
        {
            RootComposite2d.Children =
            [
                ScreenStack = new ScreenStack()
            ];
            
            ScreenStack.Push(new BaseEditorScreen());
        });
    }
}