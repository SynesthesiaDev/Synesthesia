using System.Globalization;
using Pastel;
using SDL3;

namespace Synesthesia.Engine.Logging;

public static class Logger
{
    public static bool Enabled { get; set; } = true;

    private static LogSeverity error { get; } = new("Error", ConsoleColor.Red, "#960000");
    private static LogSeverity warning { get; } = new("Warning", ConsoleColor.Yellow, "#a39800");
    private static LogSeverity debug { get; } = new("Debug");
    private static LogSeverity verbose { get; } = new("Verbose", ConsoleColor.Gray, "#004c75");

    public static LogCategory Runtime { get; } = new("Runtime");
    public static LogCategory Platform { get; } = new("Platform");

    public static LogCategory Dependency { get; } = new("Dependency");
    public static LogCategory Input { get; } = new("Input");
    public static LogCategory Audio { get; } = new("Audio");
    public static LogCategory Network { get; } = new("Network");
    public static LogCategory Render { get; } = new("Render");
    public static LogCategory Database { get; } = new("Database");
    public static LogCategory Io { get; } = new("IO");

    public record LogSeverity(string Name, ConsoleColor? ConsoleColor = null, string DebugOverlayColor = "#4f4f4f");

    public record LogCategory(string Name);

    public record LogEvent(string Message, LogSeverity Severity, LogCategory Category, bool DisplayTimestamp, Guid Uuid);

    private static void log(string message, LogSeverity severity, LogCategory category, bool displayTimestamp)
    {
        if (!Enabled) return;

        var logString = "";

        if (displayTimestamp)
        {
            var formattedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            logString += $"({formattedTime}) ";
        }

        logString += $"[{severity.Name}/{category.Name}]: {message}";

        if (severity.ConsoleColor != null)
        {
            logString = logString.Pastel(severity.ConsoleColor.Value);
        }

        Console.WriteLine(logString);
    }

    public static void Debug(string message) => log(message, debug, Runtime, true);
    public static void Verbose(string message) => log(message, verbose, Runtime, true);
    public static void Warning(string message) => log(message, warning, Runtime, true);
    public static void Error(string message) => log(message, error, Runtime, true);

    public static void Debug(string message, LogCategory category) => log(message, debug, category, true);
    public static void Verbose(string message, LogCategory category) => log(message, verbose, category, true);
    public static void Warning(string message, LogCategory category) => log(message, warning, category, true);
    public static void Error(string message, LogCategory category) => log(message, error, category, true);

    public static void Exception(Exception exception, LogCategory category)
    {
        while (true)
        {
            log(exception.ToString(), error, category, true);
            if (exception.InnerException != null)
            {
                exception = exception.InnerException;
                continue;
            }

            break;
        }
    }

    public static void SDLLog(IntPtr userData, SDL.LogCategory logCategory, SDL.LogPriority priority, string message)
    {
        Verbose(message, Platform);
    }
}
