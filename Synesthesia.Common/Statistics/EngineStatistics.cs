using Common.Util;
using SynesthesiaUtil.Types;

namespace Common.Statistics;

public static class EngineStatistics
{
    public static readonly AtomicInt BindablePools = new AtomicInt(0);
    public static readonly AtomicInt BindablesBorrowed = new AtomicInt(0);
    public static readonly AtomicInt Dispatchers = new AtomicInt(0);
    public static readonly AtomicInt DispatchersBorrowed = new AtomicInt(0);
    public static readonly AtomicInt Drawables = new AtomicInt(0);
    public static readonly AtomicInt Schedulers = new AtomicInt(0);
    public static readonly AtomicInt SchedulerTasks = new AtomicInt(0);
}