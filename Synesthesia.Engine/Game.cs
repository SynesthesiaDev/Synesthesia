// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Bindable;
using Common.Logger;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Audio;
using Synesthesia.Engine.Components.Two.Debug;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Host;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Threading.Runners;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Timing.Scheduling;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine;

public class Game : IDisposable
{
    private readonly BindablePool bindablePool = new();

    public readonly Bindable<string> WindowTitle;

    public readonly WindowsHost WindowsHost = new();

    public Scheduler GameScheduler = null!;

    public readonly AudioManager AudioManager;

    public AudioChannel MasterAudioChannel = null!;

    public AudioMixer MasterAudioMixer = null!;

    public readonly Bindable<bool> ConsumesCursor;

    public readonly DeferredActionQueue DeferredActionQueue = new();

    public readonly BindableProxy BindableProxy = new();

    public bool IsRunningInTestEnvironment => EnvUtils.IsRunningInTestEnvironment() || WindowTitle.Value == "Synesthesia Engine | Visual Tests";

    public bool CursorConsumed { get; private set; } // nom nom

    public Game()
    {
        EngineConfiguration.Load();
        DependencyContainer.Add(this);

        AudioManager = new AudioManager();
        DependencyContainer.Add(AudioManager);

        WindowTitle = bindablePool.Borrow("Synesthesia Engine");
        ConsumesCursor = bindablePool.Borrow(false);

        AudioManager.DeferredActionQueue.Enqueue(() =>
        {
            MasterAudioChannel = AudioManager.CreateChannel("Master");
            MasterAudioMixer = MasterAudioChannel.CreateMixer("master");

            MasterAudioChannel.Volume = 0.25f;
        });

        ConsumesCursor.OnValueChange(_ => updateCursorState());

        BindableProxy.Subscribe(InputManager.ON_KEY_DOWN, key =>
        {
            if (key == KeyboardKey.LeftAlt) updateCursorState();
        });

        BindableProxy.Subscribe(InputManager.ON_KEY_UP, key =>
        {
            if (key == KeyboardKey.LeftAlt) updateCursorState();
        });
    }

    private bool holdingLeftAltToEscapeCursorConsume()
    {
        return EngineConfiguration.LeftAltEscapesCursorConsume switch
        {
            false => false,
            true when KeyboardKey.LeftAlt.IsDown() => true,
            _ => false
        };
    }

    private void updateCursorState()
    {
        var renderThread = (RenderThread as RenderThreadRunner)!;
        bool shouldConsume = ConsumesCursor.Value &&
                             !holdingLeftAltToEscapeCursorConsume() &&
                             WindowsHost.WindowFocused.Value;

        renderThread.Schedule(() =>
        {
            if (shouldConsume && !CursorConsumed)
            {
                Raylib.DisableCursor();
                CursorConsumed = true;

                renderThread.PollInputEvents();
            }

            else if (!shouldConsume && CursorConsumed)
            {
                Raylib.EnableCursor();
                CursorConsumed = false;
            }
        });
    }

    public ThreadRunner InputThread { get; private set; } = null!;
    public ThreadRunner RenderThread { get; private set; } = null!;
    public ThreadRunner UpdateThread { get; private set; } = null!;
    public ThreadRunner AudioThread { get; private set; } = null!;

    public readonly DrawableScene3d RootComposite3d = new();
    public readonly CompositeDrawable2d RootComposite2d = new();
    public readonly EngineDebugOverlay EngineDebugOverlay = new();

    public readonly StopwatchClock GameRuntimeClock = new(true);

    public void Run()
    {

        ResourceManager.RegisterLoader("vsh", ResourceLoaders.LoadVertexShader, true); // Vertex Shader
        ResourceManager.RegisterLoader("fsh", ResourceLoaders.LoadFragmentShader, true); // Fragment Shader

        ResourceManager.RegisterLoader("ttf", ResourceLoaders.LoadFont, true); // Default Font (unresolved until gl initialized)

        ResourceManager.RegisterLoader("mp3", ResourceLoaders.LoadAudioSample);
        ResourceManager.RegisterLoader("ogg", ResourceLoaders.LoadAudioSample);
        ResourceManager.RegisterLoader("wav", ResourceLoaders.LoadAudioSample);

        ResourceManager.CacheAll(SynesthesiaResources.AssemblyInfo.ResourceAssembly);
        Logger.Debug($"Cached {ResourceManager.CachedSize} built-in engine resources, {ResourceManager.UnresolvedSize} waiting to be resolved, {ResourceManager.Size} total", Logger.Io);

        var loadSignal = new CountdownEvent(4);
        Action<ThreadRunner> onThreadLoaded = _ => loadSignal.Signal();

        UpdateThread = ThreadSafety.CreateThread(new UpdateThreadRunner(ThreadType.Update), ThreadSafety.THREAD_UPDATE, Defaults.UPDATE_RATE, this);
        RenderThread = ThreadSafety.CreateThread(new RenderThreadRunner(ThreadType.Draw), ThreadSafety.THREAD_RENDER, Defaults.RENDERER_RATE, this);
        InputThread = ThreadSafety.CreateThread(new InputThreadRunner(ThreadType.Input), ThreadSafety.THREAD_INPUT, Defaults.INPUT_RATE, this);
        AudioThread = ThreadSafety.CreateThread(new AudioThreadRunner(ThreadType.Audio), ThreadSafety.THREAD_AUDIO, Defaults.AUDIO_RATE, this);

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

        WindowsHost.WindowFocused.OnValueChange(_ => updateCursorState());

        Logger.Debug($"Load Complete, took {GameRuntimeClock.Elapsed.Milliseconds}ms.", Logger.Runtime);
        DeferredActionQueue.FlushAndSwitchToImmediate();

        InputThread.Thread.Join();
        RenderThread.Thread.Join();
        AudioThread.Thread.Join();
        UpdateThread.Thread.Join();
    }

    public void Dispose()
    {
        Logger.Debug("Disposing Game..", Logger.Runtime);
        BindableProxy.Dispose();
        bindablePool.Dispose();
        WindowsHost.Dispose();
        InputThread.Dispose();
        RenderThread.Dispose();
        UpdateThread.Dispose();
        AudioThread.Dispose();
        ResourceManager.ClearCache();
        Logger.Debug("Game Disposed", Logger.Runtime);
        Environment.Exit(0);
    }
}
