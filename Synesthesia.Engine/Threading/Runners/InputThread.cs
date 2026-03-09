using Common.Logger;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Threading.Runners;

public class InputThread(ThreadType type, long activeUpdateRate, long inactiveUpdateRate = 60) : ThreadRunner(type, activeUpdateRate, inactiveUpdateRate)
{
    private Game game = null!;

    protected override Logger.LogCategory GetLoggerCategory() => Logger.Input;

    protected override void OnThreadInit(Game game)
    {
        DependencyContainer.Add(this);
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
