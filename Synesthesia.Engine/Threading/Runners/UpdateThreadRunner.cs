using Common.Logger;

namespace Synesthesia.Engine.Threading.Runners;

public class UpdateThreadRunner : ThreadRunner
{
    private Game game = null!;

    protected override Logger.LogCategory GetLoggerCategory() => Logger.Runtime;

    protected override void OnThreadInit(Game game)
    {
        this.game = game;
    }

    protected override void OnLoadComplete(Game game)
    {
    }

    protected override void OnLoop()
    {
        game.RootComposite3d.OnUpdate();
        game.RootComposite2d.OnUpdate();
        game.EngineDebugOverlay.OnUpdate();
    }
}
