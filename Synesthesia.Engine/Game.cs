// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Host;
using Synesthesia.Engine.Threading.Threads;

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

    public Game(IWindowHost gameWindowHost)
    {
        WindowHost = gameWindowHost;
        InputHandler = new InputHandler();
        InputThread = new InputThread();
        UpdateThread = new UpdateThread();
        AudioThread = new AudioThread();

        DependencyContainer.Add(InputHandler);
        DependencyContainer.Add(InputThread);
        DependencyContainer.Add(WindowHost);
        //Note: RenderThread is registered as a dependency after initialization of IWindowHost
    }

    public void Run()
    {
        try
        {
            if (initialized) throw new InvalidOperationException("Game is already initialized");

            WindowHost.Initialize();
            renderThread = new RenderThread(WindowHost.Renderer);

            DependencyContainer.Add(renderThread);

            WindowHost.ReleaseGLContext();

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
