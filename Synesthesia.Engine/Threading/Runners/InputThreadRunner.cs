using Common.Logger;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Threading.Runners;

public class InputThreadRunner : IThreadRunner
{
    private Game _game = null!;

    protected override void OnThreadInit(Game game)
    {
        _game = game;
    }

    public override void OnLoadComplete(Game game)
    {
    }

    protected override void OnLoop()
    {
        try
        {
            InputManager.ProcessQueue(_game);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, Logger.INPUT);
        }
        
    }
}