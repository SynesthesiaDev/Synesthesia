namespace Synesthesia.Engine.Dependency;

public static class DependencyContainer
{
    private static Dictionary<Type, object> _cache = new();

    public static void Add<T>(T instance)
    {
        var type = typeof(T);
        _cache[type] = instance!;
    }

    public static T Get<T>() where T : class
    {
        if (!_cache.ContainsKey(typeof(T))) throw new ArgumentException($"Dependency Container does not contain {typeof(T)}");
        return (_cache[typeof(T)] as T)!;
    }
}