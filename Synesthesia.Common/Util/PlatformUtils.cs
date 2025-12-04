namespace Common.Util;

public static class PlatformUtils
{
    public static string GetPlatformName()
    {
        if (OperatingSystem.IsWindows()) return "Windows";
        if (OperatingSystem.IsLinux()) return "Linux";
        if (OperatingSystem.IsMacOS()) return "macOS";
        if (OperatingSystem.IsIOS()) return "IOS";
        return OperatingSystem.IsAndroid() ? "Android" : "Unknown";
    }
}