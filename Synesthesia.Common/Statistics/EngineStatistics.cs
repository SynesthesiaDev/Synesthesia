using SynesthesiaUtil.Types;

namespace Common.Statistics;

public static class EngineStatistics
{
    public static readonly AtomicInt DRAWABLES = new AtomicInt(0);

    public static readonly AtomicInt BINDABLE_POOLS = new AtomicInt(0);
    public static readonly AtomicInt BINDABLES_BORROWED = new AtomicInt(0);

    public static readonly AtomicInt DISPATCHERS = new AtomicInt(0);
    public static readonly AtomicInt DISPATCHERS_BORROWED = new AtomicInt(0);

    public static readonly AtomicInt SCHEDULERS = new AtomicInt(0);
    public static readonly AtomicInt ACTIVE_SCHEDULERS = new AtomicInt(0);
    public static readonly AtomicInt SCHEDULER_TASKS = new AtomicInt(0);

    public static readonly AtomicInt ANIMATORS = new AtomicInt(0);
    public static readonly AtomicInt ACTIVE_ANIMATORS = new AtomicInt(0);
    public static readonly AtomicInt ANIMATIONS = new AtomicInt(0);

    public static readonly AtomicInt AUDIO_CHANNELS = new AtomicInt(0);
    public static readonly AtomicInt AUDIO_MIXERS = new AtomicInt(0);
    public static readonly AtomicInt CACHED_AUDIO_SAMPLES = new AtomicInt(0);
    public static readonly AtomicInt AUDIO_SAMPLE_INSTANCES = new AtomicInt(0);

    public static readonly Atomic<double> BASS_CPU = new Atomic<double>(0.0);

}
