using Common.Logger;
using Common.Statistics;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Input;

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
        try
        {
            game.RootComposite3d.OnUpdate(frameInfo);
            game.RootComposite2d.OnUpdate(frameInfo);
            game.EngineDebugOverlay.OnUpdate(frameInfo);
            InputSimulator.Update(frameInfo);
        }
        finally
        {
            EngineStatistics.LAYOUT_INVALIDATIONS.Update(_ => 0);
        }

    }
}
