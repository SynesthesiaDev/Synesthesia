using System.Collections.Concurrent;
using System.Diagnostics;
using Common.Bindable;
using Common.Event;
using Common.Logger;

namespace Synesthesia.Engine.Threading.Runners;

public abstract class ThreadRunner : IDisposable
{
    public Thread Thread { get; private set; } = null!;

    protected readonly BindablePool BindablePool = new BindablePool();

    public Bindable<TimeSpan> TargetUpdateRate = null!;

    private readonly ConcurrentQueue<Action> workQueue = new();

    private Game game = null!;

    private bool isRunning;

    public readonly SingleOffEventDispatcher<ThreadRunner> ThreadLoadedDispatcher = new();

    private long fpsBits;
    private long frameTimeTicks;

    public double Fps => BitConverter.Int64BitsToDouble(Interlocked.Read(ref fpsBits));
    public TimeSpan FrameTime => new(Interlocked.Read(ref frameTimeTicks));

    public int FpsTarget = 0;

    protected void MarkLoaded()
    {
        FpsTarget = (int)Math.Floor(1.0 / TargetUpdateRate.Value.TotalSeconds);
        Logger.Debug($"{Thread.Name} thread running at {FpsTarget}hz", Logger.Io);
        ThreadLoadedDispatcher.Dispatch(this);
        OnLoadComplete(game);
    }

    protected abstract void OnLoop();

    protected abstract Logger.LogCategory GetLoggerCategory();

    protected abstract void OnThreadInit(Game game);

    protected abstract void OnLoadComplete(Game game);

    public void Schedule(Action func) => workQueue.Enqueue(func);

    private void executeScheduledActions()
    {
        while (workQueue.TryDequeue(out var workItem))
        {
            workItem.Invoke();
        }
    }

    public void Start(Thread thread, Game game)
    {
        this.game = game;
        Thread = thread;

        TargetUpdateRate = BindablePool.Borrow(TimeSpan.FromSeconds(1.0 / 60));

        isRunning = true;
        Thread.Start();
    }

    public void InternalLoop()
    {
        try
        {
            OnThreadInit(game);
            MarkLoaded();

            while (isRunning)
            {
                var frameStart = Stopwatch.GetTimestamp();


                OnLoop();
                executeScheduledActions();


                //
                var frameTime = Stopwatch.GetElapsedTime(frameStart);
                Interlocked.Exchange(ref frameTimeTicks, frameTime.Ticks);


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
                Interlocked.Exchange(ref fpsBits, BitConverter.DoubleToInt64Bits(fps));
            }
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, GetLoggerCategory());
            Environment.Exit(-1);
        }
    }

    public void Dispose()
    {
        BindablePool.Dispose();
        isRunning = false;
        workQueue.Clear();
        ThreadLoadedDispatcher.Dispose();
        Thread.Interrupt();
    }
}
