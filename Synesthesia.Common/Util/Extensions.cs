using SynesthesiaUtil.Extensions;

namespace Common.Util;

public static class Extensions
{
    public static string ObjectName(this object obj)
    {
        return obj.GetType()
            .Name;
    }

    public static int CycleIndex<T>(this List<T> list, int currentIndex)
    {
        if (list.IsEmpty()) return 0;

        return (currentIndex + 1) % list.Count;
    }

    public static List<T?> FilterNonNull<T>(this List<T?> list)
    {
        return list.Filter(p => p != null);
    }

    public static void AddAll<T>(this List<T> list, List<T> other)
    {
        list.AddRange(other);
    }

    public static string RemoveLastN(this string text, int n)
    {
        if (string.IsNullOrEmpty(text) || n <= 0)
            return text;

        return n >= text.Length ? string.Empty : text[..^n];
    }

    public static void Complete(this CompletableFuture<Nothing> completableFuture)
    {
        completableFuture.Complete(Nothing.INSTANCE);
    }
}
