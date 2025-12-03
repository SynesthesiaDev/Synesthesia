using Common.Logger;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Host;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Threading.Runners;

namespace Synesthesia.Engine;

public class Game
{
    public string WindowTitle { get; set; } = "Synesthesia Engine";
    public required IHost Host;
    public readonly RendererType Renderer = EngineEnvironment.GraphicsRendererType;

    public IThreadRunner InputThread { get; private set; } = null!;
    public IThreadRunner RenderThread { get; private set; } = null!;
    public IThreadRunner UpdateThread { get; private set; } = null!;
    public IThreadRunner AudioThread { get; private set; } = null!;

    public void Run()
    {
        Logger.Debug($"Initializing Veldrid renderer with {Renderer.ToString()} device..", Logger.RENDER);
        InputThread = ThreadSafety.CreateThread(new InputThreadRunner(), ThreadSafety.THREAD_INPUT, Defaults.InputRate, this);
        RenderThread = ThreadSafety.CreateThread(new RenderThreadRunner(), ThreadSafety.THREAD_RENDER, Defaults.RendererRate, this);
        UpdateThread = ThreadSafety.CreateThread(new UpdateThreadRunner(), ThreadSafety.THREAD_UPDATE, Defaults.UpdateRate, this);
        AudioThread = ThreadSafety.CreateThread(new AudioThreadRunner(), ThreadSafety.THREAD_AUDIO, Defaults.AudioRate, this);

        InputThread.Thread.Join();
        RenderThread.Thread.Join();
        AudioThread.Thread.Join();
        UpdateThread.Thread.Join();
    }
}