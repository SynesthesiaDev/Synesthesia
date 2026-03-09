using Synesthesia.Engine.Threading.Runners;

namespace Synesthesia.Engine.Threading;

public static class ThreadSafety
{
    public const string THREAD_INPUT = "Input";
    public const string THREAD_AUDIO = "Audio";
    public const string THREAD_RENDER = "Draw";
    public const string THREAD_UPDATE = "Update";

    public static void AssertRunningOnInputThread() => assertRunningOnThread(THREAD_INPUT);
    public static void AssertRunningOnAudioThread() => assertRunningOnThread(THREAD_AUDIO);
    public static void AssertRunningOnRenderThread() => assertRunningOnThread(THREAD_RENDER);
    public static void AssertRunningOnUpdateThread() => assertRunningOnThread(THREAD_UPDATE);

    public static bool IsUpdateThread => Thread.CurrentThread.Name == THREAD_UPDATE;
    public static bool IsRenderThread => Thread.CurrentThread.Name == THREAD_RENDER;
    public static bool IsAudioThread => Thread.CurrentThread.Name == THREAD_AUDIO;
    public static bool IsInputThread => Thread.CurrentThread.Name == THREAD_INPUT;

    public static ThreadRunner CreateThread(ThreadRunner threadRunner, Game game)
    {
        threadRunner.Start(game);
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
