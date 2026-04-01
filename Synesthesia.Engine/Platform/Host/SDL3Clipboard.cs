// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;
using System.Text;
using static SDL3.SDL;

namespace Synesthesia.Engine.Platform.Host;

public class SDL3Clipboard : IClipboard
{
    private const string text_mime_type = "text/plain;charset=utf-8";

    private static string? pendingClipboardText;

    public string? GetClipboardText()
    {
        var dataPointer = GetClipboardData(text_mime_type, out _);

        if (dataPointer == IntPtr.Zero) return null;
        var clipboardText = Marshal.PtrToStringUTF8(dataPointer);
        Free(dataPointer);

        return clipboardText;
    }

    public void SetClipboardText(string text)
    {
        pendingClipboardText = text;
        SetClipboardData(clipboardCallback, cleanupCallback, IntPtr.Zero, [text_mime_type], 1);
    }

    private static IntPtr clipboardCallback(IntPtr userdata, string mimeType, out nuint size)
    {
        if (pendingClipboardText == null || !string.Equals(mimeType, text_mime_type, StringComparison.Ordinal))
        {
            size = 0;
            return IntPtr.Zero;
        }

        byte[] bytes = Encoding.UTF8.GetBytes(pendingClipboardText);
        size = (nuint)bytes.Length;

        var pointer = Malloc(size); // wahbt the fuzck malloc in C#
        Marshal.Copy(bytes, 0, pointer, bytes.Length);

        return pointer;
    }

    private static void cleanupCallback(IntPtr userdata)
    {
        pendingClipboardText = null;
    }
}
