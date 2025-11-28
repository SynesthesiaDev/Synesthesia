using System.Diagnostics;
using Synesthesia.Engine.Threading.Runners;

namespace Synesthesia.Engine.Threading;

public static class ThreadSafety
{
    public const string THREAD_INPUT = "InputThread";
    public const string THREAD_AUDIO = "AudioThread";
    public const string THREAD_RENDER = "RenderThread";
    public const string THREAD_UPDATE = "UpdateThread";

    public static void AssertRunningOnInputThread() => assertRunningOnThread(THREAD_INPUT, "Input");
    public static void AssertRunningOnAudioThread() => assertRunningOnThread(THREAD_AUDIO, "Audio");
    public static void AssertRunningOnRenderThread() => assertRunningOnThread(THREAD_RENDER, "Render");
    public static void AssertRunningOnUpdateThread() => assertRunningOnThread(THREAD_UPDATE, "Update");

    public static IThreadRunner CreateThread(IThreadRunner threadRunner, string name, long updateTime, Game game)
    {
        var thread = new Thread(threadRunner.InternalLoop) { Name = name, IsBackground = false };
        threadRunner.SetTargetUpdateTime(updateTime);
        threadRunner.Start(thread, game);
        return threadRunner;
    }

    private static void assertRunningOnThread(string threadName, string exceptionName)
    {
        var isCorrectThread = Thread.CurrentThread.Name != threadName;
        var message = $"This action can only be performed on {exceptionName} thread!";
        Debug.Assert(isCorrectThread, message);
        if (isCorrectThread) throw new ThreadStateException(message);
    }
}