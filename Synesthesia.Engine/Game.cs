// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Host;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Resources.Stores;
using Synesthesia.Engine.Threading.Threads;
using Synesthesia.Resources;

namespace Synesthesia.Engine;

public class Game
{
    public IWindowHost WindowHost { get; }

    public InputHandler InputHandler { get; }

    public InputThread InputThread { get; }
    public UpdateThread UpdateThread { get; }
    public AudioThread AudioThread { get; }
    public RenderThread RenderThread => renderThread ?? throw new InvalidOperationException("Render Thread is still not initialized");

    private RenderThread? renderThread;

    private bool initialized;

    public readonly IResourceStore<Texture> TextureResourceStore = new ResourceStoreBuilder<Texture>()
        .AddLoaders(new Dictionary<string, Func<Stream, Texture>>
        {
            { "png", stream => ResourceLoaders.LoadTexture(stream) },
            { "bmp", stream => ResourceLoaders.LoadTexture(stream) },
        })
        .AddFallback(fallback =>
        {
            fallback.AddFileSystemStore("Assets/Textures/");
            fallback.AddAssemblyStream(AssemblyInfo.ResourceAssembly);
        })
        .MakeCached()
        .MakeAsync()
        .MakeDeferred()
        .Build();


    /// <summary>
    /// Primary game class
    /// </summary>
    /// <param name="gameWindowHost">Window Host that the game uses to create window and handle os events</param>
    public Game(IWindowHost gameWindowHost)
    {
        WindowHost = gameWindowHost;
        InputHandler = new InputHandler();
        InputThread = new InputThread();
        UpdateThread = new UpdateThread();
        AudioThread = new AudioThread();

        DependencyContainer.AddSingleton(TextureResourceStore);
        DependencyContainer.AddSingleton(InputHandler);
        DependencyContainer.AddSingleton(InputThread);
        DependencyContainer.AddSingleton(WindowHost);
        //Note: RenderThread is registered as a dependency after initialization of IWindowHost
    }

    /// <summary>
    /// Initializes Render, Update, Audio, and Input threads and creates the game window
    /// </summary>
    /// <threadsafety>Function should be called on the main STA thread</threadsafety>
    /// <exception cref="InvalidOperationException"></exception>
    public void Run()
    {
        try
        {
            if (initialized) throw new InvalidOperationException("Game is already initialized");

            WindowHost.Initialize();
            renderThread = new RenderThread(WindowHost.Renderer);

            DependencyContainer.AddSingleton(renderThread.Renderer);
            DependencyContainer.AddSingleton(renderThread);

            WindowHost.Surface.ReleaseOwnership();

            RenderThread.ActiveUpdateRate.Value = Defaults.RENDER_THREAD_HZ;
            UpdateThread.ActiveUpdateRate.Value = Defaults.UPDATE_THREAD_HZ;
            AudioThread.ActiveUpdateRate.Value = Defaults.AUDIO_THREAD_HZ;
            InputThread.ActiveUpdateRate.Value = Defaults.INPUT_THREAD_HZ;

            renderThread.Start();
            InputThread.Start();
            UpdateThread.Start();
            AudioThread.Start();

            WindowHost.ExitRequested.Subscribe(_ =>
            {
                initialized = false;
                RenderThread.Dispose();
                InputHandler.Dispose();
            });

            initialized = true;
            WindowHost.RunWindow();
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, Logger.Platform);
        }
    }
}
