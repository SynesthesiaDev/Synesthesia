using Common.Logger;
using Synesthesia.Engine.Host;
using Veldrid;

namespace Synesthesia.Engine.Threading.Runners;

public class RenderThreadRunner : IThreadRunner
{
    private IHost _host = null!;

    public static DeviceBuffer VertexBuffer;
    public static DeviceBuffer IndexBuffer;
    public static Shader[] Shaders;
    public static Pipeline Pipeline;

    protected override void OnInit(Game game)
    {
        _host = game.Host;
        Logger.Verbose($"Rendering at {Math.Round(1 / targetUpdateTime.TotalSeconds)}fps", Logger.RENDER);
    }

    protected override void OnLoop()
    {
        if(!_host.WindowExists) return;

        var commandList = _host.GetCommandList();
        commandList.Begin();
        commandList.SetFramebuffer(_host.GetGraphicsDevice().SwapchainFramebuffer);
        commandList.ClearColorTarget(0, RgbaFloat.Black);

        commandList.End();
        _host.GetGraphicsDevice().SubmitCommands(commandList);

        _host.SwapBuffers();
    }
}