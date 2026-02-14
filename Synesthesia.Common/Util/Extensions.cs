using System.Numerics;
using SynesthesiaUtil.Extensions;
using SynesthesiaUtil.Randomness;

namespace Common.Util;

public static class Extensions
{
    private static readonly Random random = new();



    public static float ToRad(this float deg)
    {
        return deg * (MathF.PI / 180f);
    }

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

    public static string ToPrettyString(this Vector3 vector3)
    {
        return $"{vector3.X:0.0}, {vector3.Y:0.0}, {vector3.Z:0.0}";
    }

    public static T RandomFixed<T>(this IList<T> source)
    {
        if (source.Count == 0)
            return default;

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
