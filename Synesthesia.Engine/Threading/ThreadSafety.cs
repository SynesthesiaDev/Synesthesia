// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Synesthesia.Engine.Threading;

public static class ThreadSafety
{
    [Conditional("DEBUG")]
    private static void assertRunningOnThread(string threadName)
    {
        var isNotCorrectThread = !string.Equals(Thread.CurrentThread.Name, threadName, StringComparison.Ordinal);
        var message = $"This action can only be performed on {threadName} thread!";
        if (isNotCorrectThread) throw new ThreadStateException(message);
    }

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertRunningOnInputThread() => assertRunningOnThread(nameof(ThreadType.Input));

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertRunningOnAudioThread() => assertRunningOnThread(nameof(ThreadType.Audio));

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertRunningOnRenderThread() => assertRunningOnThread(nameof(ThreadType.Draw));

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertRunningOnUpdateThread() => assertRunningOnThread(nameof(ThreadType.Update));

    public static bool IsUpdateThread => string.Equals(Thread.CurrentThread.Name, nameof(ThreadType.Update), StringComparison.Ordinal);
    public static bool IsRenderThread => string.Equals(Thread.CurrentThread.Name, nameof(ThreadType.Draw), StringComparison.Ordinal);
    public static bool IsAudioThread => string.Equals(Thread.CurrentThread.Name, nameof(ThreadType.Audio), StringComparison.Ordinal);
    public static bool IsInputThread => string.Equals(Thread.CurrentThread.Name, nameof(ThreadType.Input), StringComparison.Ordinal);
}
