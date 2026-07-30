// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Threading.Threads;
using Synesthesia.Engine.Util.Statistics;
using Synesthesia.Utils.Extensions;
using Synesthesia.Utils.Types;

namespace Synesthesia.Engine.Timing;

public class Scheduler : IDisposable
{
    private readonly Timer timer;
    private long currentTime;
    private readonly Stopwatch stopwatch = new();

    private readonly Lock timerLock = new();
    private bool timerRunning;

    [Singleton]
    private UpdateThread updateThread = null!;

    public Scheduler()
    {
        stopwatch.Start();
        timer = new Timer(tick, null, Timeout.Infinite, Timeout.Infinite);
        timerRunning = false;

        EngineStatistics.Increment(EngineStatistics.Type.Schedulers);
        Reflection.ResolveDependencies(this);
    }

    public void CancelAllTasks()
    {
        foreach (var scheduledTask in scheduledTasks.SelectMany(keyValuePair => keyValuePair.Value))
        {
            scheduledTask.Dispose();
        }

        foreach (var repeatingTask in repeatingTasks.SelectMany(keyValuePair => keyValuePair.Value))
        {
            repeatingTask.Dispose();
        }

        scheduledTasks.Clear();
        repeatingTasks.Clear();
    }

    private void wakeUp()
    {
        lock (timerLock)
        {
            if (timerRunning) return;
            timer.Change(0, 1);
            timerRunning = true;
        }
    }

    private void stopTimerIfIdle()
    {
        lock (timerLock)
        {
            if (!timerRunning) return;

            if (scheduledTasks.Keys.Count != 0 || repeatingTasks.Keys.Count != 0) return;

            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timerRunning = false;
        }
    }

    private void tick(object? state)
    {
        var now = stopwatch.ElapsedMilliseconds;
        Interlocked.Exchange(ref currentTime, now);

        if (scheduledTasks.Keys.Count == 0 && repeatingTasks.Keys.Count == 0)
        {
            stopTimerIfIdle();
            return;
        }

        updateThread.Schedule(() =>
        {
            handleScheduledTasks();
            handleRepeatingTasks();
        });
    }

    private readonly NestedValueMap<long, ScheduledTask> scheduledTasks = new();

    private readonly NestedValueMap<long, RepeatingTask> repeatingTasks = new();

    private void handleScheduledTasks()
    {
        var now = Interlocked.Read(ref currentTime);

        var tasksToHandle = scheduledTasks
            .Keys.Where(k => k <= now)
            .ToList();
        if (tasksToHandle.IsEmpty()) return;

        foreach (ref long timeKey in CollectionsMarshal.AsSpan(tasksToHandle))
        {
            if (!scheduledTasks.Remove(timeKey, out var tasks)) continue;

            foreach (ref ScheduledTask task in CollectionsMarshal.AsSpan(tasks))
            {
                if (!task.CancellationToken.IsCancellationRequested)
                {
                    task.Action.Invoke(task);
                }

                task.Dispose();
            }
        }

        scheduledTasks.Remove(now);
    }

    private void handleRepeatingTasks()
    {
        var now = Interlocked.Read(ref currentTime);
        var intervals = repeatingTasks.Keys.ToList();

        foreach (ref long interval in CollectionsMarshal.AsSpan(intervals))
        {
            var list = repeatingTasks.Get(interval);
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var t = list[i];
                if (!t.CancellationToken.IsCancellationRequested) continue;

                t.Dispose();
                list.RemoveAt(i);
            }

            foreach (ref RepeatingTask task in CollectionsMarshal.AsSpan(list))
            {
                if (task.NextRunTime == 0) task.NextRunTime = now + task.Interval;

                if (now < task.NextRunTime) continue;

                task.Iteration++;
                task.Action.Invoke(task);
                task.NextRunTime += task.Interval;
            }

            if (list.Count == 0) repeatingTasks.Remove(interval);
        }
    }

    public ScheduledTask Schedule(long time, Action<ScheduledTask> action)
    {
        var now = Interlocked.Read(ref currentTime);
        var task = new ScheduledTask(this, false, time, action, new CancellationTokenSource());
        scheduledTasks.AddValue(now + time, task);
        EngineStatistics.Increment(EngineStatistics.Type.SchedulerTasks);

        wakeUp();

        return task;
    }

    public RepeatingTask Repeating(long interval, Action<RepeatingTask> action)
    {
        var task = new RepeatingTask(this, 0, interval, action, new CancellationTokenSource());
        repeatingTasks.AddValue(interval, task);
        EngineStatistics.Increment(EngineStatistics.Type.SchedulerTasks);

        wakeUp();

        return task;
    }

    public RepeatingTask Iterate(long iterations, long interval, Action<IterativeTask> action, Action then)
    {
        var currentLoop = 0L;

        wakeUp();

        return Repeating(interval, repeating =>
        {
            if (currentLoop >= iterations)
            {
                return;
            }

            currentLoop++;
            var loopsLeft = iterations - currentLoop;
            var isLast = currentLoop >= iterations;
            var iterativeTask = new IterativeTask(this, currentLoop, loopsLeft, isLast, repeating, repeating.CancellationToken);

            action.Invoke(iterativeTask);

            if (!isLast) return;

            repeating.Dispose();
            then.Invoke();
        });
    }

    public void Dispose()
    {
        lock (timerLock)
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timerRunning = false;
        }

        foreach (var scheduledTask in scheduledTasks.SelectMany(keyValuePair => keyValuePair.Value))
        {
            scheduledTask.Dispose();
        }

        foreach (var repeatingTask in repeatingTasks.SelectMany(keyValuePair => keyValuePair.Value))
        {
            repeatingTask.Dispose();
        }

        EngineStatistics.Decrement(EngineStatistics.Type.Schedulers);
        scheduledTasks.Clear();
        repeatingTasks.Clear();
        timer.Dispose();
    }

    public interface ITask : IDisposable
    {
        Scheduler Parent { get; }
        CancellationTokenSource CancellationToken { get; }
        CancellationToken Token => CancellationToken.Token;

        bool AlreadyDisposed { get; set; }
    }

    public record ScheduledTask(Scheduler Parent, bool HasBeenRun, long ScheduledTime, Action<ScheduledTask> Action, CancellationTokenSource CancellationToken) : ITask
    {
        public bool HasBeenRun { get; set; } = HasBeenRun;

        public bool AlreadyDisposed { get; set; }

        public void Dispose()
        {
            if(AlreadyDisposed) return;
            AlreadyDisposed = true;

            if (!CancellationToken.IsCancellationRequested)
                CancellationToken.Cancel();

            CancellationToken.Dispose();
            EngineStatistics.Decrement(EngineStatistics.Type.SchedulerTasks);
        }
    }

    public record RepeatingTask(
        Scheduler Parent,
        long Iteration,
        long Interval,
        Action<RepeatingTask> Action,
        CancellationTokenSource CancellationToken
    ) : ITask
    {
        public long Iteration { get; set; } = Iteration;
        public long NextRunTime { get; set; }

        public bool AlreadyDisposed { get; set; }

        public void Dispose()
        {
            if(AlreadyDisposed) return;
            AlreadyDisposed = true;


            if (!CancellationToken.IsCancellationRequested)
                CancellationToken.Cancel();


            CancellationToken.Dispose();
            EngineStatistics.Decrement(EngineStatistics.Type.SchedulerTasks);
        }
    }

    public record IterativeTask(
        Scheduler Parent,
        long CurrentIteration,
        long IterationsLeft,
        bool IsLastIteration,
        RepeatingTask InnerTask,
        CancellationTokenSource CancellationToken
    ) : ITask
    {
        public bool AlreadyDisposed { get; set; }

        public void Dispose()
        {
            if(AlreadyDisposed) return;
            AlreadyDisposed = true;

            if (!CancellationToken.IsCancellationRequested)
                CancellationToken.Cancel();

            CancellationToken.Dispose();
            EngineStatistics.Decrement(EngineStatistics.Type.SchedulerTasks);
        }
    }
}
