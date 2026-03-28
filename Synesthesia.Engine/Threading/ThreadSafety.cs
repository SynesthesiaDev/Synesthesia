// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Synesthesia.Engine.Threading;

public static class ThreadSafety
{

    [ThreadStatic]
    private static bool isInputThread;

    [ThreadStatic]
    private static bool isAudioThread;

    [ThreadStatic]
    private static bool isUpdateThread;

    [ThreadStatic]
    private static bool isRenderThread;

    public static void SetThreadType(ThreadType type)
    {
        isUpdateThread = type == ThreadType.Update;
        isRenderThread = type == ThreadType.Draw;
        isAudioThread = type == ThreadType.Audio;
        isInputThread = type == ThreadType.Input;
    }

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertRunningOnInputThread()
    {
        if (!isInputThread) throw new ThreadStateException("This action can only be performed on Input thread!");
    }

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertRunningOnAudioThread()
    {
        if (!isAudioThread) throw new ThreadStateException("This action can only be performed on Audio thread!");
    }
    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertRunningOnRenderThread()
    {
        if (!isRenderThread) throw new ThreadStateException("This action can only be performed on Render thread!");
    }

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertRunningOnUpdateThread()
    {
        if (!isUpdateThread) throw new ThreadStateException("This action can only be performed on Update thread!");
    }

    public static bool IsUpdateThread => isUpdateThread;
    public static bool IsRenderThread => isRenderThread;
    public static bool IsAudioThread => isAudioThread;
    public static bool IsInputThread => isInputThread;
}
