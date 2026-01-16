using SynesthesiaUtil.Extensions;

namespace Common.Util;

public static class Extensions
{
    public static string ObjectName(this object obj)
    {
        return obj.GetType().Name;
    }

    public static List<T?> FilterNonNull<T>(this List<T?> list)
    {
        return list.Filter(p => p != null);
    }
}