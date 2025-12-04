using Common.Logger;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Host;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Threading.Runners;

namespace Synesthesia.Engine;

public class Game : IDisposable
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

        // Input Thread creates the window, so we need it initialized before doing anything else rendering-wise
        InputThread.ThreadLoadedDispatcher.Subscribe(_ =>
        {
            InputThread.OnLoadComplete(this);
            RenderThread.OnLoadComplete(this);
            UpdateThread.OnLoadComplete(this);
            AudioThread.OnLoadComplete(this);
        });

        Logger.Debug("Initializing Resource Loaders..", Logger.IO);
        ResourceManager.RegisterLoader("vsh", ResourceLoaders.LoadText); // Vertex Shader
        ResourceManager.RegisterLoader("fsh", ResourceLoaders.LoadText); // Fragment Shader

        Logger.Debug("Caching built-in resources..", Logger.IO);
        var cachedResources = ResourceManager.CacheAll(SynesthesiaResources.AssemblyInfo.ResourceAssembly);
        Logger.Debug($"Cached {cachedResources} resources", Logger.IO);

        InputThread.Thread.Join();
        RenderThread.Thread.Join();
        AudioThread.Thread.Join();
        UpdateThread.Thread.Join();
    }

    public void Dispose()
    {
        InputThread.Dispose();
        RenderThread.Dispose();
        UpdateThread.Dispose();
        AudioThread.Dispose();
        ResourceManager.ClearCache();
    }
}