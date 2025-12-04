using System.Numerics;
using Common.Logger;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Host;
using Veldrid;
using ResourceManager = Synesthesia.Engine.Resources.ResourceManager;

namespace Synesthesia.Engine.Threading.Runners;

public class RenderThreadRunner : IThreadRunner
{
    private IHost _host = null!;
    private GraphicsDevice _graphicsDevice = null!;
    private ResourceFactory _resourceFactory = null!;

    public static DeviceBuffer VertexBuffer = null!;
    public static DeviceBuffer IndexBuffer = null!;
    public static Shader[] Shaders = null!;
    public static Pipeline Pipeline = null!;

    VertexPositionColor[] quadVertices =
    [
        new VertexPositionColor(new Vector2(-0.75f, 0.75f), RgbaFloat.Red),
        new VertexPositionColor(new Vector2(0.75f, 0.75f), RgbaFloat.Green),
        new VertexPositionColor(new Vector2(-0.75f, -0.75f), RgbaFloat.Blue),
        new VertexPositionColor(new Vector2(0.75f, -0.75f), RgbaFloat.Yellow)
    ];

    ushort[] quadIndices = [0, 1, 2, 3];

    protected override void OnThreadInit(Game game)
    {
        _host = game.Host;
        MarkLoaded();
    }

    public override void OnLoadComplete(Game game)
    {
        _graphicsDevice = _host.GetGraphicsDevice();
        _resourceFactory = _graphicsDevice.ResourceFactory;

        VertexBuffer = _resourceFactory.CreateBuffer(new BufferDescription(4 * VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer));
        IndexBuffer = _resourceFactory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));

        _graphicsDevice.UpdateBuffer(VertexBuffer, 0, quadVertices);

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4)
        );

        Logger.Verbose("Loading built-in shaders..", Logger.RENDER);
        var vertexShader = ResourceManager.Get<string>("SynesthesiaResources.main.vsh");
        var fragmentShader = ResourceManager.Get<string>("SynesthesiaResources.main.fsh");

        Logger.Verbose($"Rendering at {Math.Round(1 / targetUpdateTime.TotalSeconds)}fps", Logger.RENDER);
    }

    protected override void OnLoop()
    {
        if (!_host.WindowExists) return;

        var commandList = _host.GetCommandList();
        commandList.Begin();
        commandList.SetFramebuffer(_host.GetGraphicsDevice().SwapchainFramebuffer);
        commandList.ClearColorTarget(0, RgbaFloat.Black);

        commandList.End();
        _host.GetGraphicsDevice().SubmitCommands(commandList);

        _host.SwapBuffers();
    }
}