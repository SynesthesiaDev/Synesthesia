using System.Numerics;
using System.Text;
using Common.Logger;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Host;
using SynesthesiaUtil;
using Veldrid;
using Veldrid.SPIRV;
using ResourceManager = Synesthesia.Engine.Resources.ResourceManager;

namespace Synesthesia.Engine.Threading.Runners;

public class RenderThreadRunner : IThreadRunner
{
    private IHost _host = null!;
    private Game _game = null!;
    private GraphicsDevice _graphicsDevice = null!;
    private ResourceFactory _resourceFactory = null!;

    public static DeviceBuffer VertexBuffer = null!;
    public static DeviceBuffer IndexBuffer = null!;
    public static List<Shader> Shaders = null!;
    public static Pipeline? Pipeline;

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
        _game = game;
        MarkLoaded();
    }

    public override void OnLoadComplete(Game game)
    {
        _graphicsDevice = _host.GetGraphicsDevice();
        _resourceFactory = _graphicsDevice.ResourceFactory;

        VertexBuffer = _resourceFactory.CreateBuffer(new BufferDescription(4 * VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer));
        IndexBuffer = _resourceFactory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));

        _graphicsDevice.UpdateBuffer(VertexBuffer, 0, quadVertices);
        _graphicsDevice.UpdateBuffer(IndexBuffer, 0, quadIndices);

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        );

        Logger.Verbose("Loading built-in shaders..", Logger.RENDER);
        var vertexShader = ResourceManager.Get<string>("SynesthesiaResources.main.vsh");
        var fragmentShader = ResourceManager.Get<string>("SynesthesiaResources.main.fsh");

        var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertexShader), "main");
        var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragmentShader), "main");

        _resourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
        Shaders = Lists.Of(_resourceFactory.CreateShader(vertexShaderDesc), _resourceFactory.CreateShader(fragmentShaderDesc));

        var pipelineDescription = new GraphicsPipelineDescription
        {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new DepthStencilStateDescription(depthTestEnabled: true, depthWriteEnabled: true, comparisonKind: ComparisonKind.LessEqual),
            RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: true
            ),
            PrimitiveTopology = PrimitiveTopology.TriangleStrip,
            ResourceLayouts = [],
            ShaderSet = new ShaderSetDescription(vertexLayouts: [vertexLayout], shaders: Shaders.ToArray()),
            Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription
        };

        Pipeline = _resourceFactory.CreateGraphicsPipeline(pipelineDescription);

        Logger.Verbose($"Rendering at {Math.Round(1 / targetUpdateTime.TotalSeconds)}fps", Logger.RENDER);
    }

    protected override void OnLoop()
    {
        if (!_host.WindowExists) return;
        if(Pipeline == null) return;

        var commandList = _host.GetCommandList();
        commandList.Begin();
        commandList.SetVertexBuffer(0, VertexBuffer);
        commandList.SetFramebuffer(_host.GetGraphicsDevice().SwapchainFramebuffer);
        commandList.SetIndexBuffer(IndexBuffer, IndexFormat.UInt16);
        // commandList.ClearColorTarget(0, RgbaFloat.Black);
        commandList.SetPipeline(Pipeline);
        commandList.DrawIndexed(4, 1, 0, 0, 0);

        commandList.End();
        _host.GetGraphicsDevice().SubmitCommands(commandList);

        _host.SwapBuffers();
    }
}