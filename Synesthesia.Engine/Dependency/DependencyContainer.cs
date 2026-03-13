
using Synesthesia.Engine.Logging;

namespace Synesthesia.Engine.Dependency;

public static class DependencyContainer
{
    private static readonly Dictionary<Type, object> cache = new();

    public static void Add<T>(T instance)
    {
        var type = typeof(T);
        cache[type] = instance!;
        Logger.Verbose($"Added {type.Name} to dependency cache", Logger.Dependency);
    }

    public static object Get(Type type)
    {
        cache.TryGetValue(type, out var value);
        if (value != null)
        {
            EngineStatistics.DEPENDENCIES_RESOLVED.Increment();
            return value;
        }

        var message = $"Dependency Container does not contain {type}";
        Logger.Error(message, Logger.Dependency);
        throw new ArgumentException(message, nameof(type));
    }

    public static T Get<T>() where T : class
    {
        return (Get(typeof(T)) as T)!;
    }
}
