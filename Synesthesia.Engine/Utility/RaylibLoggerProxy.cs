using System.Runtime.InteropServices;
using Raylib_cs;

namespace Synesthesia.Engine.Utility;

public static unsafe class RaylibLoggerProxy
{
    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static unsafe void HandleRaylibLog(int msgType, sbyte* text, sbyte* args)
    {
        var message = Logging.GetLogMessage(new IntPtr(text), new IntPtr(args));
        // Logger.Verbose($"{message}", Logger.RENDER);
    }
}