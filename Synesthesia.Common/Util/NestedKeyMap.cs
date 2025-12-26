namespace Common.Util;

public class NestedValueMap<K, V> : Dictionary<K, List<V>> where K : notnull
{
    public void AddValue(K key, V value)
    {
        if (!TryGetValue(key, out var list))
        {
            list = new List<V>();
            this[key] = list;
        }

        list.Add(value);
    }

    public List<V> Get(K key)
    {
        return this.GetOrNull(key) ?? [];
    }
}