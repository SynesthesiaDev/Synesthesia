using Common.Logger;
using Synesthesia.Engine.Host;

namespace Synesthesia.Engine.Threading.Runners;

public class AudioThreadRunner : IThreadRunner
{
    private IHost _host = null!;

    protected override void OnInit(Game game)
    {
        _host = game.Host;
        Logger.Verbose($"Audio thread running at {Math.Round(1 / targetUpdateTime.TotalSeconds)}hz", Logger.AUDIO);
    }

    protected override void OnLoop()
    {
    }
}