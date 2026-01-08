using System.Numerics;
using Common.Bindable;
using Common.Logger;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Components.Two.Debug;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Host;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Threading.Runners;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Timing.Scheduling;

namespace Synesthesia.Engine;

public class Game : IDisposable
{
    private BindablePool _bindablePool = new();

    public readonly Bindable<string> WindowTitle;

    public bool ConsumesMouse = false;

    public readonly WindowHost WindowHost = new();

    public Scheduler GameScheduler = null!;

    public Game()
    {
        WindowTitle = _bindablePool.Borrow("Synesthesia Engine");
    }


    public IThreadRunner InputThread { get; private set; } = null!;
    public IThreadRunner RenderThread { get; private set; } = null!;
    public IThreadRunner UpdateThread { get; private set; } = null!;
    public IThreadRunner AudioThread { get; private set; } = null!;

    public readonly CompositeDrawable3d RootComposite3d = new();
    public readonly CompositeDrawable2d RootComposite2d = new();
    public readonly EngineDebugOverlay EngineDebugOverlay = new();

    public readonly StopwatchClock GameRuntimeClock = new(true);

    public void Run()
    {
        EngineConfiguration.Load();

        DependencyContainer.Add(this);

        ResourceManager.RegisterLoader("vsh", ResourceLoaders.LoadVertexShader, true); // Vertex Shader
        ResourceManager.RegisterLoader("fsh", ResourceLoaders.LoadFragmentShader, true); // Fragment Shader
        ResourceManager.RegisterLoader("ttf", ResourceLoaders.LoadFont,
            true); // Default Font (unresolved until gl initialized)

        ResourceManager.CacheAll(SynesthesiaResources.AssemblyInfo.ResourceAssembly);
        Logger.Debug(
            $"Cached {ResourceManager.CachedSize} built-in engine resources, {ResourceManager.UnresolvedSize} waiting to be resolved, {ResourceManager.Size} total",
            Logger.IO);

        var loadSignal = new CountdownEvent(4);
        Action<IThreadRunner> onThreadLoaded = _ => loadSignal.Signal();

        UpdateThread = ThreadSafety.CreateThread(new UpdateThreadRunner(), ThreadSafety.THREAD_UPDATE,
            Defaults.UpdateRate, this);
        RenderThread = ThreadSafety.CreateThread(new RenderThreadRunner(), ThreadSafety.THREAD_RENDER,
            Defaults.RendererRate, this);
        InputThread =
            ThreadSafety.CreateThread(new InputThreadRunner(), ThreadSafety.THREAD_INPUT, Defaults.InputRate, this);
        AudioThread =
            ThreadSafety.CreateThread(new AudioThreadRunner(), ThreadSafety.THREAD_AUDIO, Defaults.AudioRate, this);

        UpdateThread.ThreadLoadedDispatcher.Subscribe(onThreadLoaded);
        RenderThread.ThreadLoadedDispatcher.Subscribe(onThreadLoaded);
        InputThread.ThreadLoadedDispatcher.Subscribe(onThreadLoaded);
        AudioThread.ThreadLoadedDispatcher.Subscribe(onThreadLoaded);

        DependencyContainer.Add((RenderThread as RenderThreadRunner)!);
        DependencyContainer.Add((InputThread as InputThreadRunner)!);
        DependencyContainer.Add((UpdateThread as UpdateThreadRunner)!);
        DependencyContainer.Add((AudioThread as AudioThreadRunner)!);

        loadSignal.Wait();
        GameScheduler = new Scheduler();

        EngineDebugOverlay.Load();
        RootComposite2d.Load();
        RootComposite3d.Load();

        Logger.Debug($"Load Complete, took {GameRuntimeClock.Elapsed.Milliseconds}ms.", Logger.RUNTIME);

        RootComposite2d.Children =
        [
            new FillFlowContainer2d
            {
                AutoSizeAxes = Axes.Both,
                Direction = Direction.Vertical,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = 5,
                Children =
                [
                    new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Horizontal,
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Spacing = 5,
                        Children =
                        [
                            new DefaultEngineButton
                            {
                                Size = new Vector2(120, 40),
                                Text = "Disabled :c",
                                Disabled = true
                            },

                            new DefaultEngineButton
                            {
                                Size = new Vector2(120, 40),
                                Text = "Clicky Clack",
                            },

                            new DefaultEngineButton
                            {
                                Size = new Vector2(120, 40),
                                Text = "Click 2",
                                TextColor = Color.Black,
                                ColorCombination = DefaultEngineColorCombination.Accent
                            },
                        ]
                    },
                    
                    new DefaultEngineCheckbox
                    {
                        Text = "Do Stuff",
                        Size = new Vector2(200, 30),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    
                    new DefaultEngineCheckbox
                    {
                        Text = "Also disabled :c",
                        Size = new Vector2(200, 30),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Disabled = true,
                    }
                ],
            },
        ];

        InputThread.Thread.Join();
        RenderThread.Thread.Join();
        AudioThread.Thread.Join();
        UpdateThread.Thread.Join();
    }

    public void Dispose()
    {
        Logger.Debug("Disposing Game..", Logger.RUNTIME);
        _bindablePool.Dispose();
        WindowHost.Dispose();
        InputThread.Dispose();
        RenderThread.Dispose();
        UpdateThread.Dispose();
        AudioThread.Dispose();
        ResourceManager.ClearCache();
        Logger.Debug("Game Disposed", Logger.RUNTIME);
        Environment.Exit(0);
    }
}