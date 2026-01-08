using System.Diagnostics;
using Common.Statistics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Threading.Runners;
using SynesthesiaUtil.Extensions;
using SynesthesiaUtil.Types;

namespace Synesthesia.Engine.Timing.Scheduling;

public class Scheduler : IDisposable
{
    private Timer timer;
    private long _currentTime;
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private UpdateThreadRunner UpdateThreadRunner = DependencyContainer.Get<UpdateThreadRunner>();

    public Scheduler()
    {
        _stopwatch.Start();
        timer = new Timer(Tick, null, 0, 1);
        EngineStatistics.Schedulers.Increment();
    }

    private void Tick(object? state)
    {
        var now = _stopwatch.ElapsedMilliseconds;
        Interlocked.Exchange(ref _currentTime, now);

        UpdateThreadRunner.Schedule(() =>
        {
            handleScheduledTasks();
            handleRepeatingTasks();
        });
    }

    private NestedValueMap<long, ScheduledTask> _scheduledTasks = new();

    private NestedValueMap<long, RepeatingTask> _repeatingTasks = new();

    private void handleScheduledTasks()
    {
        var now = Interlocked.Read(ref _currentTime);

        var tasksToHandle = _scheduledTasks.Keys.Where(k => k <= now).ToList();
        if (tasksToHandle.IsEmpty()) return;

        foreach (var timeKey in tasksToHandle)
        {
            if (!_scheduledTasks.Remove(timeKey, out var tasks)) continue;

            foreach (var task in tasks)
            {
                if (!task.CancellationToken.IsCancellationRequested)
                {
                    task.Action.Invoke(task);
                }

                task.Dispose();
            }
        }

        _scheduledTasks.Remove(now);
    }

    private void handleRepeatingTasks()
    {
        var now = Interlocked.Read(ref _currentTime);
        var intervals = _repeatingTasks.Keys.ToList();

        foreach (var list in intervals.Select(interval => _repeatingTasks.Get(interval)))
        {
            list.Filter(t => t.CancellationToken.IsCancellationRequested).ForEach(t => t.Dispose());

            foreach (var task in list)
            {
                if (task.NextRunTime == 0) task.NextRunTime = now + task.Interval;

                if (now < task.NextRunTime) continue;

                task.Iteration++;
                task.Action.Invoke(task);

                task.NextRunTime += task.Interval;
            }
        }
    }

    public ScheduledTask Schedule(long time, Action<ScheduledTask> action)
    {
        var now = Interlocked.Read(ref _currentTime);
        var task = new ScheduledTask(this, false, time, action, new CancellationTokenSource());
        _scheduledTasks.AddValue(now + time, task);
        EngineStatistics.SchedulerTasks.Increment();
        return task;
    }

    public RepeatingTask Repeating(long interval, Action<RepeatingTask> action)
    {
        var task = new RepeatingTask(this, 0, interval, action, new CancellationTokenSource());
        _repeatingTasks.AddValue(interval, task);
        EngineStatistics.SchedulerTasks.Increment();

        return task;
    }

    public RepeatingTask Iterate(long iterations, long interval, Action<IterativeTask> action, Action then)
    {
        var currentLoop = 0L;
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
        foreach (var scheduledTask in _scheduledTasks.SelectMany(keyValuePair => keyValuePair.Value))
        {
            scheduledTask.CancellationToken.Dispose();
        }

        foreach (var repeatingTask in _repeatingTasks.SelectMany(keyValuePair => keyValuePair.Value))
        {
            repeatingTask.CancellationToken.Dispose();
        }

        EngineStatistics.Schedulers.Decrement();
        _scheduledTasks.Clear();
        _repeatingTasks.Clear();
        timer.Dispose();
    }

    public interface ITask : IDisposable
    {
        public Scheduler Parent { get; }
        public CancellationTokenSource CancellationToken { get; }
        public CancellationToken Token => CancellationToken.Token;
    }

    public record ScheduledTask(Scheduler Parent, bool HasBeenRun, long ScheduledTime, Action<ScheduledTask> Action, CancellationTokenSource CancellationToken) : ITask
    {
        public bool HasBeenRun { get; set; } = HasBeenRun;

        public void Dispose()
        {
            CancellationToken.Dispose();
            EngineStatistics.SchedulerTasks.Decrement();
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

        public void Dispose()
        {
            CancellationToken.Dispose();
            EngineStatistics.SchedulerTasks.Decrement();
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
        public void Dispose()
        {
            CancellationToken.Dispose();
            EngineStatistics.SchedulerTasks.Decrement();
        }
    }
}