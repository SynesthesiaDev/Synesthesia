using SynesthesiaUtil.Types;

namespace Common.Statistics;

public static class EngineStatistics
{
    public static readonly AtomicInt DRAWABLES = new(0);

    public static readonly AtomicInt BINDABLE_POOLS = new(0);
    public static readonly AtomicInt BINDABLES_BORROWED = new(0);

    public static readonly AtomicInt DISPATCHERS = new(0);
    public static readonly AtomicInt DISPATCHERS_BORROWED = new(0);

    public static readonly AtomicInt SCHEDULERS = new(0);
    public static readonly AtomicInt ACTIVE_SCHEDULERS = new(0);
    public static readonly AtomicInt SCHEDULER_TASKS = new(0);

    public static readonly AtomicInt ANIMATORS = new(0);
    public static readonly AtomicInt ACTIVE_ANIMATORS = new(0);
    public static readonly AtomicInt ANIMATIONS = new(0);

    public static readonly AtomicInt AUDIO_CHANNELS = new(0);
    public static readonly AtomicInt AUDIO_MIXERS = new(0);
    public static readonly AtomicInt CACHED_AUDIO_SAMPLES = new(0);
    public static readonly AtomicInt AUDIO_SAMPLE_INSTANCES = new(0);

    public static readonly Atomic<double> BASS_CPU = new(0.0);

}
