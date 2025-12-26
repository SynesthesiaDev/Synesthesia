namespace Common.Util;

public static class Extensions
{
    public static T? LastOrNull<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.Any() ? enumerable.Last() : default;
    }

    public static string CutIfTooLong(this string text, int maxLenght, bool threeDots = true)
    {
        return text.Length >= maxLenght ? $"{text.Substring(0, maxLenght)}" + (threeDots ? "..." : "") : text;
    }

    public static V? GetOrNull<K, V>(this Dictionary<K, V> dictionary, K key) where K : notnull
    {
        return dictionary.GetValueOrDefault(key);
    }
}