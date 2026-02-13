using Common.Statistics;

namespace Synesthesia.Engine.Dependency;

public static class DependencyContainer
{
    private static readonly Dictionary<Type, object> cache = new();

    public static void Add<T>(T instance)
    {
        var type = typeof(T);
        cache[type] = instance!;
    }

    public static object Get(Type type)
    {
        cache.TryGetValue(type, out var value);
        if (value != null)
        {
            EngineStatistics.DEPENDENCIES_RESOLVED.Increment();
            return value;
        }

        throw new ArgumentException($"Dependency Container does not contain {type}");
    }

    public static T Get<T>() where T : class
    {
        return (Get(typeof(T)) as T)!;
    }
}
