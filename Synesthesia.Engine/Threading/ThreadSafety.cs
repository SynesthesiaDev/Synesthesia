using System.Diagnostics;
using Synesthesia.Engine.Threading.Runners;

namespace Synesthesia.Engine.Threading;

public static class ThreadSafety
{
    public const string THREAD_INPUT = "Input";
    public const string THREAD_AUDIO = "Audio";
    public const string THREAD_RENDER = "Render";
    public const string THREAD_UPDATE = "Update";

    public static void AssertRunningOnInputThread() => assertRunningOnThread(THREAD_INPUT);
    public static void AssertRunningOnAudioThread() => assertRunningOnThread(THREAD_AUDIO);
    public static void AssertRunningOnRenderThread() => assertRunningOnThread(THREAD_RENDER);
    public static void AssertRunningOnUpdateThread() => assertRunningOnThread(THREAD_UPDATE);

    public static bool IsUpdateThread => Thread.CurrentThread.Name == THREAD_UPDATE;
    public static bool IsRenderThread => Thread.CurrentThread.Name == THREAD_RENDER;
    public static bool IsAudioThread => Thread.CurrentThread.Name == THREAD_AUDIO;
    public static bool IsInputThread => Thread.CurrentThread.Name == THREAD_INPUT;

    public static IThreadRunner CreateThread(IThreadRunner threadRunner, string name, long updateTime, Game game)
    {
        var thread = new Thread(threadRunner.InternalLoop) { Name = name, IsBackground = false };
        threadRunner.Start(thread, game);
        threadRunner.TargetUpdateRate.Value = TimeSpan.FromSeconds(1.0 / updateTime);
        return threadRunner;
    }

    private static void assertRunningOnThread(string threadName)
    {
        var isNotCorrectThread = Thread.CurrentThread.Name != threadName;
        var message = $"This action can only be performed on {threadName} thread!";
        // Debug.Assert(isNotCorrectThread, message);
        if (isNotCorrectThread) throw new ThreadStateException(message);
    }
}