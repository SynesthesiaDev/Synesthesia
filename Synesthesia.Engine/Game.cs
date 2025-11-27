using Common.Logger;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Host;

namespace Synesthesia.Engine;

public class Game
{
    public string WindowTitle { get; set; } = "Synesthesia Engine";
    public required IHost Host;
    private bool _isRunning;
    public RendererType Renderer = EngineEnvironment.GraphicsRendererType;

    //
    private Thread _inputThread = null!;
    // private Thread _updateThread = null!;
    // private Thread _renderThread = null!;
    // private Thread _audioThread = null!;

    public int TargetUpdateRate { get; set; } = Defaults.UpdateRate;
    public int TargetRenderRate { get; set; } = Defaults.RendererRate;
    public int TargetAudioRate { get; set; } = Defaults.AudioRate;
    public int TargetInputRate { get; set; } = Defaults.AudioRate;

    public void Run()
    {
        _isRunning = true;

        Logger.Debug($"Initializing Veldrid renderer with {Renderer.ToString()} device..", Logger.RENDER);
        _inputThread = new Thread(InputLoop) { Name = "InputThread", IsBackground = false };

        _inputThread.Start();

        _inputThread.Join();
    }

    private void InputLoop()
    {
        var targetFrameTime = TimeSpan.FromSeconds(1.0 / TargetInputRate);

        Logger.Debug($"Using {Host.GetHostName()} ({Host.GetPlatformName()}) host..", Logger.RENDER);
        Host.Initialize(this, Renderer);
        Logger.Debug($"Renderer initialized", Logger.RENDER);

        Logger.Verbose($"Input Thread running on {TargetInputRate}fps", Logger.INPUT);

        while (_isRunning)
        {
            var now = DateTime.UtcNow;

            try
            {
                Host.PollEvents();
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception: {ex.Message}", Logger.INPUT);
            }

            var elapsed = DateTime.UtcNow - now;
            var sleepTime = targetFrameTime - elapsed;
            if (sleepTime > TimeSpan.Zero)
            {
                Thread.Sleep(sleepTime);
            }
        }
    }
}