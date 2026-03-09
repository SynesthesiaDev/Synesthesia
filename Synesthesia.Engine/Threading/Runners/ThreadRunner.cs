using System.Collections.Concurrent;
using Common.Bindable;
using Common.Event;
using Common.Logger;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Threading.Runners;

public abstract class ThreadRunner(ThreadType type, long activeUpdateRate, long inactiveUpdateRate = 60) : IDisposable
{
    public Thread Thread { get; private set; } = null!;

    public readonly Bindable<long> ActiveUpdateRate = new(activeUpdateRate);

    public readonly Bindable<long> InactiveUpdateRate = new(inactiveUpdateRate);

    public readonly  Bindable<bool> IsActive = new(true);

    private readonly FrameStatistics frameStatistics = new();

    private readonly ConcurrentQueue<Action> workQueue = new();

    private readonly ThreadRunningClock clock = new();

    private Game game = null!;

    private volatile bool isRunning;
    public ulong FrameIndex { get; private set; }

    public ThreadType ThreadType => type;

    public readonly SingleOffEventDispatcher<ThreadRunner> ThreadLoadedDispatcher = new();

    public double Fps => frameStatistics.FramesPerSecond;

    public double FrameTime => frameStatistics.AverageFrameTime;

    protected void MarkLoaded()
    {
        Logger.Debug($"{Thread.Name} thread running at {ActiveUpdateRate.Value}hz", Logger.Runtime);
        ThreadLoadedDispatcher.Dispatch(this);
        OnLoadComplete(game);
    }

    protected abstract void OnLoop(FrameInfo frameInfo);

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

    public void Start(Game gameHost)
    {
        game = gameHost;
        Thread = new Thread(InternalLoop)
        {
            Name = type.ToString(),
            IsBackground = true,
        };

        ActiveUpdateRate.OnValueChange(e =>
        {
            if (IsActive.Value)
                clock.MaximumUpdateHz = e.NewValue;
        }, true);

        InactiveUpdateRate.OnValueChange(e =>
        {
            if (!IsActive.Value)
                clock.MaximumUpdateHz = e.NewValue;
        });

        IsActive.OnValueChange(e =>
        {
            clock.MaximumUpdateHz = e.NewValue ? ActiveUpdateRate.Value : InactiveUpdateRate.Value;
        });

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
                frameStatistics.Add(clock.ElapsedFrameTime);
                clock.ProcessFrame();

                FrameIndex++;

                var frameInfo = new FrameInfo
                {
                    Delta = clock.ElapsedFrameTime,
                    Type = ThreadType,
                    Time = clock.CurrentTime,
                    FrameIndex = FrameIndex
                };

                OnLoop(frameInfo);
                executeScheduledActions();
            }
        }

        catch (Exception ex)
        {
            Logger.Error($"Exception on {type.ToString()} thread:", GetLoggerCategory());
            Logger.Exception(ex, GetLoggerCategory());
#if DEBUG
            Environment.Exit(ex.HResult);
#endif
        }
    }

    public void Dispose()
    {
        isRunning = false;
        clock.Dispose();
        ActiveUpdateRate.Dispose();
        InactiveUpdateRate.Dispose();
        IsActive.Dispose();
        isRunning = false;
        workQueue.Clear();
        ThreadLoadedDispatcher.Dispose();

        if (Thread.IsAlive)
        {
            Thread.Join(TimeSpan.FromSeconds(5));
        }
    }
}
