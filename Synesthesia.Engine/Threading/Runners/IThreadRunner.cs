using System.Collections.Concurrent;
using System.Diagnostics;
using Common.Bindable;
using Common.Event;
using Common.Logger;

namespace Synesthesia.Engine.Threading.Runners;

public abstract class IThreadRunner : IDisposable
{
    public Thread Thread { get; private set; } = null!;

    protected readonly BindablePool _bindablePool = new BindablePool();

    public Bindable<TimeSpan> TargetUpdateRate = null!;

    private readonly ConcurrentQueue<Action> _workQueue = new();

    private Game _game = null!;

    private bool _isRunning;

    public readonly SingleOffEventDispatcher<IThreadRunner> ThreadLoadedDispatcher = new();

    private long _fpsBits;
    private long _frameTimeTicks;

    public double Fps => BitConverter.Int64BitsToDouble(Interlocked.Read(ref _fpsBits));
    public TimeSpan FrameTime => new(Interlocked.Read(ref _frameTimeTicks));

    public int FpsTarget = 0;

    protected void MarkLoaded()
    {
        FpsTarget = (int)Math.Floor(1.0 / TargetUpdateRate.Value.TotalSeconds);
        Logger.Debug($"{Thread.Name} thread running at {FpsTarget}hz", Logger.IO);
        ThreadLoadedDispatcher.Dispatch(this);
        OnLoadComplete(_game);
    }

    protected abstract void OnLoop();

    protected abstract void OnThreadInit(Game game);

    public abstract void OnLoadComplete(Game game);

    public void Schedule(Action func) => _workQueue.Enqueue(func);

    private void ExecuteScheduledActions()
    {
        while (_workQueue.TryDequeue(out var workItem))
        {
            workItem.Invoke();
        }
    }

    public void Start(Thread thread, Game game)
    {
        _game = game;
        Thread = thread;

        TargetUpdateRate = _bindablePool.Borrow(TimeSpan.FromSeconds(1.0 / 60));

        _isRunning = true;
        Thread.Start();
    }

    public void InternalLoop()
    {
        OnThreadInit(_game);
        MarkLoaded();

        while (_isRunning)
        {
            var frameStart = Stopwatch.GetTimestamp();

            try
            {
                OnLoop();
                ExecuteScheduledActions();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, Logger.RENDER);
                Environment.Exit(-1);
            }

            var frameTime = Stopwatch.GetElapsedTime(frameStart);
            Interlocked.Exchange(ref _frameTimeTicks, frameTime.Ticks);


            var target = TargetUpdateRate.Value;
            if (target > TimeSpan.Zero)
            {
                while (true)
                {
                    var elapsed = Stopwatch.GetElapsedTime(frameStart);
                    var remaining = target - elapsed;

                    if (remaining <= TimeSpan.Zero)
                        break;

                    if (remaining > TimeSpan.FromMilliseconds(2))
                    {
                        Thread.Sleep(remaining - TimeSpan.FromMilliseconds(1));
                    }
                    else
                    {
                        Thread.SpinWait(50);
                    }
                }
            }

            var current = Stopwatch.GetElapsedTime(frameStart);
            var fps = current.TotalSeconds > 0 ? 1.0 / current.TotalSeconds : 0.0;
            Interlocked.Exchange(ref _fpsBits, BitConverter.DoubleToInt64Bits(fps));
        }
    }

    public void Dispose()
    {
        _bindablePool.Dispose();
        _isRunning = false;
        _workQueue.Clear();
        ThreadLoadedDispatcher.Dispose();
        Thread.Interrupt();
    }
}