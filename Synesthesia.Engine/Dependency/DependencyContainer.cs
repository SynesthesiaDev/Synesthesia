namespace Synesthesia.Engine.Dependency;

public static class DependencyContainer
{
    private static Dictionary<Type, object> cache = new();

    public static void Add<T>(T instance)
    {
        var type = typeof(T);
        cache[type] = instance!;
    }

    public static T Get<T>() where T : class
    {
        if (!cache.ContainsKey(typeof(T))) throw new ArgumentException($"Dependency Container does not contain {typeof(T)}");
        return (cache[typeof(T)] as T)!;
    }
}