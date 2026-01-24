using System.Diagnostics;
using Common.Statistics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Threading.Runners;
using SynesthesiaUtil.Extensions;
using SynesthesiaUtil.Types;

namespace Synesthesia.Engine.Timing.Scheduling;

public class Scheduler : IDisposable
{
    private readonly Timer timer;
    private long currentTime;
    private readonly Stopwatch stopwatch = new Stopwatch();
    private readonly UpdateThreadRunner updateThreadRunner = DependencyContainer.Get<UpdateThreadRunner>();

    public Scheduler()
    {
        stopwatch.Start();
        timer = new Timer(tick, null, 0, 1);
        EngineStatistics.SCHEDULERS.Increment();
    }

    private void tick(object? state)
    {
        var now = stopwatch.ElapsedMilliseconds;
        Interlocked.Exchange(ref currentTime, now);

        updateThreadRunner.Schedule(() =>
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

        foreach (var timeKey in tasksToHandle)
        {
            if (!scheduledTasks.Remove(timeKey, out var tasks)) continue;

            foreach (var task in tasks)
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

        foreach (var list in intervals.Select(interval => repeatingTasks.Get(interval)))
        {
            list
                .Filter(t => t.CancellationToken.IsCancellationRequested)
                .ForEach(t => t.Dispose());

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
        var now = Interlocked.Read(ref currentTime);
        var task = new ScheduledTask(this, false, time, action, new CancellationTokenSource());
        scheduledTasks.AddValue(now + time, task);
        EngineStatistics.SCHEDULER_TASKS.Increment();
        return task;
    }

    public RepeatingTask Repeating(long interval, Action<RepeatingTask> action)
    {
        var task = new RepeatingTask(this, 0, interval, action, new CancellationTokenSource());
        repeatingTasks.AddValue(interval, task);
        EngineStatistics.SCHEDULER_TASKS.Increment();

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
        foreach (var scheduledTask in scheduledTasks.SelectMany(keyValuePair => keyValuePair.Value))
        {
            scheduledTask.CancellationToken.Dispose();
        }

        foreach (var repeatingTask in repeatingTasks.SelectMany(keyValuePair => keyValuePair.Value))
        {
            repeatingTask.CancellationToken.Dispose();
        }

        EngineStatistics.SCHEDULERS.Decrement();
        scheduledTasks.Clear();
        repeatingTasks.Clear();
        timer.Dispose();
    }

    public interface ITask : IDisposable
    {
        Scheduler Parent { get; }
        CancellationTokenSource CancellationToken { get; }
        CancellationToken Token => CancellationToken.Token;
    }

    public record ScheduledTask(Scheduler Parent, bool HasBeenRun, long ScheduledTime, Action<ScheduledTask> Action, CancellationTokenSource CancellationToken) : ITask
    {
        public bool HasBeenRun { get; set; } = HasBeenRun;

        public void Dispose()
        {
            CancellationToken.Dispose();
            EngineStatistics.SCHEDULER_TASKS.Decrement();
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
            EngineStatistics.SCHEDULER_TASKS.Decrement();
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
            EngineStatistics.SCHEDULER_TASKS.Decrement();
        }
    }
}
