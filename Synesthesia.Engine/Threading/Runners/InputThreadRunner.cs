using Common.Event;
using Common.Logger;
using Synesthesia.Engine.Host;

namespace Synesthesia.Engine.Threading.Runners;

public class InputThreadRunner : IThreadRunner
{
    private IHost _host = null!;

    protected override void OnThreadInit(Game game)
    {
        _host = game.Host;
        Logger.Debug($"Using {game.Host.GetHostName()} ({game.Host.GetPlatformName()}) host..", Logger.RENDER);
        game.Host.Initialize(game, game.Renderer);
        Logger.Debug($"Renderer initialized", Logger.RENDER);

        Logger.Verbose($"Input polling at {1.0 / targetUpdateTime.TotalSeconds}hz", Logger.INPUT);

        MarkLoaded();
    }

    public override void OnLoadComplete(Game game)
    {
    }

    protected override void OnLoop()
    {
        _host.PollEvents();
    }
}