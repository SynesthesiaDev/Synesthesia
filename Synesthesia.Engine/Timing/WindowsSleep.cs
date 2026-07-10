// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Synesthesia.Engine.Timing;

public class WindowsSleep : INativeSleep
{
    private IntPtr waitableTimer;

    public WindowsSleep()
    {
        create();
    }

    [SuppressMessage("ErrorHandling", "ERP022:Unobserved exception in a generic exception handler")]
    private void create()
    {
        try
        {
            // Attempt to use CREATE_WAITABLE_TIMER_HIGH_RESOLUTION, only available since Windows 10, version 1803.
            waitableTimer = CreateWaitableTimerEx(IntPtr.Zero, null,
                CreateWaitableTimerFlags.CreateWaitableTimerManualReset | CreateWaitableTimerFlags.CreateWaitableTimerHighResolution, TIMER_ALL_ACCESS);

            if (waitableTimer == IntPtr.Zero)
            {
                // Fall back to a more supported version. This is still far more accurate than Thread.Sleep.
                waitableTimer = CreateWaitableTimerEx(IntPtr.Zero, null, CreateWaitableTimerFlags.CreateWaitableTimerManualReset, TIMER_ALL_ACCESS);
            }
        }
        catch
        {
            // Any kind of unexpected exception should fall back to Thread.Sleep.
        }
    }

    public bool Sleep(TimeSpan duration)
    {
        if (waitableTimer == IntPtr.Zero) return false;

        if (!SetWaitableTimerEx(waitableTimer, CreateFileTime(duration), 0, routine: null, 0, IntPtr.Zero, 0)) return false;

        WaitForSingleObject(waitableTimer, INFINITE);
        return true;
    }

    public void Dispose()
    {
        if (waitableTimer == IntPtr.Zero) return;

        CloseHandle(waitableTimer);
        waitableTimer = IntPtr.Zero;
    }

    [DllImport("kernel32.dll")]
    internal static extern bool SetWaitableTimerEx(IntPtr hTimer, in FILETIME lpDueTime, int lPeriod, TimerApcProc? routine, IntPtr lpArgToCompletionRoutine, IntPtr reason, uint tolerableDelay);

    [DllImport("kernel32.dll")]
    internal static extern IntPtr CreateWaitableTimerEx(IntPtr lpTimerAttributes, string? lpTimerName, CreateWaitableTimerFlags dwFlags, uint dwDesiredAccess);

    internal const uint TIMER_ALL_ACCESS = 2031619U;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate void TimerApcProc([In] IntPtr lpArgToCompletionRoutine, uint dwTimerLowValue, uint dwTimerHighValue);

    [Flags]
    internal enum CreateWaitableTimerFlags : uint
    {
        CreateWaitableTimerManualReset = 0x00000001,
        CreateWaitableTimerHighResolution = 0x00000002,
    }

    public const uint INFINITE = 0xffffffff;

    [DllImport("kernel32.dll")]
    internal static extern bool WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    internal static FILETIME CreateFileTime(TimeSpan ts)
    {
        ulong ul = unchecked((ulong)-ts.Ticks);
        return new FILETIME { dwHighDateTime = (int)(ul >> 32), dwLowDateTime = (int)(ul & 0xFFFFFFFF) };
    }

    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hObject);
}
