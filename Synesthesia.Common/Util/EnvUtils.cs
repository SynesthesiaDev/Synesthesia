using SynesthesiaUtil.Extensions;

namespace Common.Util;

public static class EnvUtils
{
    public static bool GetBool(string name, bool def)
    {
        return (Environment.GetEnvironmentVariable(name) ?? def.ToString()).ToBoolean();
    }

    public static string GetString(string name, string def)
    {
        return (Environment.GetEnvironmentVariable(name) ?? def);
    }

    public static float GetFloat(string name, float def)
    {
        return (Environment.GetEnvironmentVariable(name) ?? def.ToString()).ToFloat();
    }

    public static double GetDouble(string name, double def)
    {
        return (Environment.GetEnvironmentVariable(name) ?? def.ToString()).ToDouble();
    }

    public static T GetEnum<T>(string name, T def) where T : struct
    {
        var env = Environment.GetEnvironmentVariable(name);
        return env == null ? def : Enum.Parse<T>(env);
    }

    public static void Set(string name, object value)
    {
        Environment.SetEnvironmentVariable(name, value.ToString());
    }

    public static bool IsRunningInTestEnvironment()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName != null &&
                assembly.FullName.StartsWith("nunit.framework", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}