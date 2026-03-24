using SynesthesiaUtil.Types;

namespace Synesthesia.Engine;

public static class EngineStatistics
{
    public static readonly AtomicInt DRAWABLES = new(0);

    public static readonly AtomicInt BINDABLE_SCOPES = new(0);
    public static readonly AtomicInt BINDABLE_SCOPE_BORROWS = new(0);

    public static readonly AtomicInt DISPATCHERS = new(0);
    public static readonly AtomicInt DISPATCHERS_BORROWED = new(0);

    public static readonly AtomicInt SCHEDULERS = new(0);
    public static readonly AtomicInt SCHEDULER_TASKS = new(0);

    public static readonly AtomicInt AUDIO_CHANNELS = new(0);
    public static readonly AtomicInt AUDIO_MIXERS = new(0);
    public static readonly AtomicInt AUDIO_SAMPLE_INSTANCES = new(0);

    public static readonly AtomicInt DEPENDENCIES_RESOLVED = new(0);
    public static readonly AtomicInt DEPENDENCIES_RESOLVED_REFLECTION = new(0);

    public static readonly AtomicInt OBJECTS_RENTED = new(0);
    public static readonly AtomicInt OBJECTS_RETURNED = new(0);
    public static readonly AtomicInt OBJECTS_ALIVE = new(0);

    public static readonly AtomicInt TOTAL_TABLET_EVENTS = new(0);

    public static readonly Atomic<double> BASS_CPU = new(0.0);
}
