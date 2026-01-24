using Common.Event;
using Common.Util;
using Pastel;
using SynesthesiaUtil.Types;

namespace Common.Logger;

public static class Logger
{
    public static bool Enabled { get; set; } = true;

    public static AtomicInt LogCount { get; } = new(0);

    public static readonly EventDispatcher<LogEvent> MESSAGE_LOGGED = new();

    private static LogSeverity error { get; } = new LogSeverity("Error", ConsoleColor.Red, "#960000");
    private static LogSeverity warning { get; } = new LogSeverity("Warning", ConsoleColor.Yellow, "#a39800");
    private static LogSeverity debug { get; } = new LogSeverity("Debug");
    private static LogSeverity verbose { get; } = new LogSeverity("Verbose", ConsoleColor.Gray, "#004c75");

    public static LogCategory Runtime { get; } = new LogCategory("Runtime");
    public static LogCategory Input { get; } = new LogCategory("Input");
    public static LogCategory Audio { get; } = new LogCategory("Audio");
    public static LogCategory Network { get; } = new LogCategory("Network");
    public static LogCategory Render { get; } = new LogCategory("Render");
    public static LogCategory Database { get; } = new LogCategory("Database");
    public static LogCategory Io { get; } = new LogCategory("IO");

    public record LogSeverity(string Name, ConsoleColor? ConsoleColor = null, string DebugOverlayColor = "#4f4f4f");

    public record LogCategory(string Name);

    public record LogEvent(string Message, LogSeverity Severity, LogCategory Category, bool DisplayTimestamp, Guid Uuid);

    private static void log(string message, LogSeverity severity, LogCategory category, bool displayTimestamp)
    {
        if (!Enabled) return;

        var logString = "";

        if (displayTimestamp)
        {
            var formattedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            logString += $"({formattedTime}) ";
        }

        logString += $"[{severity.Name}/{category.Name}]: {message}";

        if (severity.ConsoleColor != null)
        {
            logString = logString.Pastel(severity.ConsoleColor.Value);
        }

        Console.WriteLine(logString);
        MESSAGE_LOGGED.Dispatch(new LogEvent(message, severity, category, displayTimestamp, Guid.NewGuid()));
        LogCount.Increment();
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
        log(exception.ToString(), error, category, true);
        if (exception.InnerException != null)
        {
            Exception(exception.InnerException, category);
        }
    }
}