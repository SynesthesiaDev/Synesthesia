// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util.Bindables;

namespace Synesthesia.Engine.Threading;

public abstract class ThreadRunner : IDisposable
{
    private const int default_active_update_rate = 1000;
    private const int default_inactive_update_rate = 60;

    public Thread Thread { get; private set; } = null!;

    public readonly Bindable<long> ActiveUpdateRate = new(default_active_update_rate);

    public readonly Bindable<long> InactiveUpdateRate = new(default_inactive_update_rate);

    public readonly Bindable<bool> IsActive = new(true);

    private readonly FrameStatistics frameStatistics = new();

    private readonly ThreadRunningClock clock = new();

    private volatile bool isRunning;

    public ulong FrameIndex { get; private set; }

    public abstract ThreadType Type { get; }

    public double Fps => frameStatistics.FramesPerSecond;

    public double FrameTime => frameStatistics.AverageFrameTime;

    protected abstract void ProcessFrame(FrameInfo frameInfo);

    protected abstract void OnThreadInit();

    private readonly ConcurrentQueue<Action> commandQueue = new();

    public void Schedule(Action action) => commandQueue.Enqueue(action);

    protected abstract Logger.LogCategory LoggerCategory { get; }

    private void tryDequeueCommand()
    {
        while (commandQueue.TryDequeue(out var workItem))
        {
            workItem.Invoke();
        }
    }

    [Singleton]
    private Game game = null!;

    public void Start()
    {
        Reflection.ResolveDependencies(this);
        Thread = new Thread(InternalLoop)
        {
            Name = Type.ToString(),
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
        }, true);

        IsActive.BindTo(game.WindowHost.WindowActive);

        isRunning = true;
        Thread.Start();
    }

    public void InternalLoop()
    {
        try
        {
            OnThreadInit();
            Logger.Debug($"{Type} Thread polling at {ActiveUpdateRate.Value}hz", LoggerCategory);

            while (isRunning)
            {
                frameStatistics.Add(clock.ElapsedFrameTime);
                clock.ProcessFrame();

                FrameIndex++;

                var frameInfo = new FrameInfo
                {
                    Delta = clock.ElapsedFrameTime,
                    Type = Type,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Time = clock.CurrentTime,
                    FrameIndex = FrameIndex,
                };

                tryDequeueCommand();
                ProcessFrame(frameInfo);
            }
        }

        catch (Exception ex)
        {
            Logger.Error($"Exception on {Type.ToString()} thread:", LoggerCategory);
            Logger.Exception(ex, LoggerCategory);
#if DEBUG
            Environment.Exit(ex.HResult);
#endif
        }
    }

    public void Dispose()
    {
        isRunning = false;
        ActiveUpdateRate.Dispose();
        InactiveUpdateRate.Dispose();
        IsActive.Dispose();
        clock.Dispose();
        commandQueue.Clear();

        if (Thread.IsAlive)
        {
            Thread.Join(TimeSpan.FromSeconds(5));
        }
    }
}
