using Common.Logger;
using Synesthesia.Engine.Host;

namespace Synesthesia.Engine.Threading.Runners;

public class RenderThreadRunner : IThreadRunner
{
    private IHost _host = null!;

    protected override void OnInit(Game game)
    {
        _host = game.Host;
        Logger.Verbose($"Rendering at {Math.Round(1 / targetUpdateTime.TotalSeconds)}fps", Logger.RENDER);
    }

    protected override void OnLoop()
    {
        _host.SwapBuffers();
    }
}