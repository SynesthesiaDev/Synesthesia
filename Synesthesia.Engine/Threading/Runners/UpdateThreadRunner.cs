using Common.Logger;

namespace Synesthesia.Engine.Threading.Runners;

public class UpdateThreadRunner : IThreadRunner
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
        _game.RootComposite3d.OnUpdate();
        _game.RootComposite2d.OnUpdate();
        _game.EngineDebugOverlay.OnUpdate();
    }
}