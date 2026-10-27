using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Util.Statistics;

public static class EngineStatistics
{
    private static readonly long[] statistics = new long[Enum.GetValues<Type>().Length];

    public static readonly Snapshot LATEST_SNAPSHOT = new Snapshot();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Increment(Type type) => Interlocked.Increment(ref statistics[(int)type]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(Type type, long amount) => Interlocked.Exchange(ref statistics[(int)type], amount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Decrement(Type type) => Interlocked.Decrement(ref statistics[(int)type]);

    public static void Reset() => Array.Clear(statistics, 0, statistics.Length);

    public static long Get(Type type) => Interlocked.Read(ref statistics[(int)type]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TakeSnapshot() => LATEST_SNAPSHOT.Set(statistics);

    [StructLayout(LayoutKind.Auto)]
    public readonly struct Snapshot()
    {
        private readonly long[] statistics = new long[Enum.GetValues<Type>().Length];

        public void Set(long[] newStats) => newStats.AsSpan().CopyTo(statistics);
        public long Get(Type type) => statistics[(int)type];
    }


    public enum Type
    {
        Drawables,
        Bindables,
        Dispatchers,
        Schedulers,
        SchedulerTasks,
        Animators,
        Animations,
        AudioChannels,
        AudioMixers,
        AudioSamples,
        DependenciesResolved,
        ObjectsRented,
        ObjectsReturned,
        ObjectsInflight,
        TabletEvents,
        TextureAtlases,
        FontAtlases,
        CachedResourceStoreSize,
        ColorCacheSize,
    }
}
