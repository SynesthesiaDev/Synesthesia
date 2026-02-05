using SynesthesiaUtil.Extensions;
using SynesthesiaUtil.Randomness;

namespace Common.Util;

public static class Extensions
{
    private static readonly Random random = new Random();

    public static List<T> Shuffled<T>(this IList<T> list)
    {
        var newList = new List<T>(list);

        int n = newList.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (newList[k], newList[n]) = (newList[n], newList[k]);
        }

        return newList;
    }

    public static T RandomFixed<T>(this IList<T> source)
    {
        // 1. Guard against empty lists to avoid crashes
        if (source.Count == 0)
            return default; // Or throw an exception depending on your needs

        // 2. Ensure the upper bound is correctly handled
        // If RNG.RandomInt is exclusive, source.Count is correct.
        // If RNG.RandomInt is inclusive, you must use source.Count - 1.
        int index = RNG.RandomInt(0, source.Count);

        return source[index];
    }

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
