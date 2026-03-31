// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Events;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Host;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Resources.Stores;
using Synesthesia.Engine.Threading.Threads;
using Synesthesia.Engine.Util.Future;
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

    public readonly SingleOffEventDispatcher<Game> OnInitialized = new SingleOffEventDispatcher<Game>();

    /// <summary>
    /// Resource store which contains Textures. Is <see cref="CachedResourceStore{Texture}"/>, <see cref="AsyncResourceStore{Texture}"/>, and <see cref="DeferredResourceStore{Texture}"/>.
    /// </summary>
    /// <remarks>
    /// The deferred store is unlocked after the <see cref="RenderThread"/> is fully loaded.
    /// </remarks>
    /// <exception cref="FileNotFoundException">File was not found</exception>
    /// <exception cref="InvalidOperationException">Deferred store is not ready yet</exception>
    /// <example>
    /// <code>
    /// [Singleton]
    /// private IResourceStore&lt;Texture&gt; textureResourceStore = null!;
    /// <br></br>
    /// protected override void OnLoading() {
    ///     textureResourceStore.Get("Synesthesia.Resources.Textures.dull_blade.png")
    /// }
    ///
    /// </code>
    /// </example>
    public readonly IResourceStore<Texture> TextureResourceStore = new ResourceStoreBuilder<Texture>()
        .AddLoaders(new Dictionary<string, Func<Stream, string, Texture>>
        {
            { "png", (stream, _) => ResourceLoaders.LoadTexture(stream) },
            { "bmp", (stream, _) => ResourceLoaders.LoadTexture(stream) },
            { "jpg", (stream, _) => ResourceLoaders.LoadTexture(stream) },
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

    public readonly IResourceStore<TextureAtlas> TextureAtlasResourceStore = new ResourceStoreBuilder<TextureAtlas>()
        .AddLoaders(new Dictionary<string, Func<Stream, string, TextureAtlas>>
        {
            { ResourceLoaders.TEXTURE_ATLAS_FILE_EXT, (stream, _) => ResourceLoaders.LoadFromTextureAtlasFile(stream) },
        })
        .AddFallback(fallback =>
        {
            fallback.AddFileSystemStore("Assets/Atlases/");
            fallback.AddAssemblyStream(AssemblyInfo.ResourceAssembly);
        })
        .Build();


    /// <summary>
    /// Resource store which contains Fonts. Is <see cref="CachedResourceStore{Font}"/>, <see cref="AsyncResourceStore{Font}"/>, and <see cref="DeferredResourceStore{Font}"/>.
    /// </summary>
    /// <remarks>
    /// The deferred store is unlocked after the <see cref="RenderThread"/> is fully loaded.
    /// </remarks>
    /// <exception cref="FileNotFoundException">File was not found</exception>
    /// <exception cref="InvalidOperationException">Deferred store is not ready yet</exception>
    /// <example>
    /// <code>
    /// [Singleton]
    /// private IResourceStore&lt;Font&gt; fontResourceStore = null!;
    /// <br></br>
    /// protected override void OnLoading() {
    ///     fontResourceStore.Get("Synesthesia.Resources.Font.Quicksand-regular.ttf")
    /// }
    ///
    /// </code>
    /// </example>
    public readonly IResourceStore<Font> FontResourceStore = new ResourceStoreBuilder<Font>()
        .AddLoaders(new Dictionary<string, Func<Stream, string, Font>>
        {
            { "ttf", ResourceLoaders.LoadFont },
            { ResourceLoaders.FONT_ATLAS_FILE_EXT, ResourceLoaders.LoadFontFromAtlas },
        })
        .AddFallback(fallback =>
        {
            fallback.AddFileSystemStore("Assets/Fonts/");
            fallback.AddAssemblyStream(AssemblyInfo.ResourceAssembly);
        })
        .MakeCached()
        .MakeAsync()
        .MakeDeferred()
        .Build();

    private readonly InternalGameContainer2d internalGameContainer2d = new InternalGameContainer2d();

    /// <summary>
    /// This is the main 2d scene where you add your drawables.
    /// While <see cref="DrawableScene2d.AddChild"/> exists, please do so by overriding the children field directly
    /// </summary>
    /// <code>
    /// DrawableScene2d.Children =
    /// [
    ///     new Text2d
    ///     {
    ///         Text = "Hello World!"
    ///     }
    /// ];
    /// </code>
    public DrawableScene2d DrawableScene2d => internalGameContainer2d.DrawableScene2d;

    /// <summary>
    /// Primary game class
    /// </summary>
    /// <param name="gameWindowHost">Window Host that the game uses to create window and handle os events</param>
    public Game(IWindowHost gameWindowHost)
    {
        WindowHost = gameWindowHost;
        InputHandler = new InputHandler(this);
        InputThread = new InputThread();
        UpdateThread = new UpdateThread();
        AudioThread = new AudioThread();

        DependencyContainer.AddSingleton(this);
        DependencyContainer.AddSingleton(FontResourceStore);
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

            CompletableFuture.All(RenderThread.LoadFuture, UpdateThread.LoadFuture, AudioThread.LoadFuture, InputThread.LoadFuture).Then(_ =>
            {
                initialized = true;
                OnInitialized.Dispatch(this);
            });

            WindowHost.RunWindow();
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, Logger.Platform);
        }
    }

    public InternalGameContainer2d GetInternalGameContainer() => internalGameContainer2d;
}
