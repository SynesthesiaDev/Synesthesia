using Common.Logger;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Threading.Runners;

public class InputThreadRunner(ThreadType type) : ThreadRunner(type)
{
    private Game game = null!;

    protected override Logger.LogCategory GetLoggerCategory() => Logger.Input;

    protected override void OnThreadInit(Game game)
    {
        this.game = game;
    }

    protected override void OnLoadComplete(Game game)
    {
    }

    protected override void OnLoop(FrameInfo frameInfo)
    {
        try
        {
            InputManager.ProcessQueue(game);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, Logger.Input);
        }

    }
}
