using Common.Util;
using Pastel;

namespace Common.Logger;

public static class Logger
{
    public static bool Enabled { get; set; } = true;

    public static AtomicInt LogCount { get; } = new(0);

    private static LogSeverity ERROR { get; } = new LogSeverity("Error", ConsoleColor.Red);
    private static LogSeverity WARNING { get; } = new LogSeverity("Warning", ConsoleColor.Yellow);
    private static LogSeverity DEBUG { get; } = new LogSeverity("Debug");
    private static LogSeverity VERBOSE { get; } = new LogSeverity("Verbose");

    public static LogType RUNTIME { get; } = new LogType("Runtime");
    public static LogType INPUT { get; } = new LogType("Input");
    public static LogType AUDIO { get; } = new LogType("Audio");
    public static LogType NETWORK { get; } = new LogType("Network");
    public static LogType RENDER { get; } = new LogType("Render");
    public static LogType DATABASE { get; } = new LogType("Database");
    public static LogType IO { get; } = new LogType("IO");

    private record LogSeverity(string name, ConsoleColor? consoleColor = null);

    public record LogType(string name);

    private static void log(string message, LogSeverity severity, LogType type, bool displayTimestamp)
    {
        if (!Enabled) return;

        var logString = "";

        if (displayTimestamp)
        {
            var formattedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            logString += $"({formattedTime}) ";
        }

        logString += $"[{severity.name}/{type.name}]: {message}";

        if (severity.consoleColor != null)
        {
            logString = logString.Pastel(severity.consoleColor.Value);
        }

        Console.WriteLine(logString);
        LogCount.Increment();
    }

    public static void Debug(string message) => log(message, DEBUG, RUNTIME, true);
    public static void Verbose(string message) => log(message, VERBOSE, RUNTIME, true);
    public static void Warning(string message) => log(message, WARNING, RUNTIME, true);
    public static void Error(string message) => log(message, ERROR, RUNTIME, true);

    public static void Debug(string message, LogType type) => log(message, DEBUG, type, true);
    public static void Verbose(string message, LogType type) => log(message, VERBOSE, type, true);
    public static void Warning(string message, LogType type) => log(message, WARNING, type, true);
    public static void Error(string message, LogType type) => log(message, ERROR, type, true);

    public static void Exception(Exception exception, LogType type)
    {
        log(exception.ToString(), ERROR, type, true);
        if (exception.InnerException != null)
        {
            Exception(exception.InnerException, type);
        }
    }
}