using Common.Logger;
using Synesthesia.Engine.Host;

namespace Synesthesia.Engine.Threading.Runners;

public class UpdateThreadRunner : IThreadRunner
{
    private IHost _host = null!;

    protected override void OnInit(Game game)
    {
        _host = game.Host;
        Logger.Verbose($"Update thread running at {Math.Round(1 / targetUpdateTime.TotalSeconds)}hz", Logger.RUNTIME);
    }

    protected override void OnLoop()
    {
    }
}