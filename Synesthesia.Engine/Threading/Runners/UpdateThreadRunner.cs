using Common.Logger;
using Synesthesia.Engine.Graphics;

namespace Synesthesia.Engine.Threading.Runners;

public class UpdateThreadRunner(ThreadType type) : ThreadRunner(type)
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

    protected override void OnLoop(FrameInfo frameInfo)
    {
        game.RootComposite3d.OnUpdate(frameInfo);
        game.RootComposite2d.OnUpdate(frameInfo);
        game.EngineDebugOverlay.OnUpdate(frameInfo);
    }
}
