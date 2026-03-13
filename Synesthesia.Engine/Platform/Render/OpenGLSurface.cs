// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Extensions;
using static SDL3.SDL;

namespace Synesthesia.Engine.Platform.Render;

public sealed class OpenGLSurface : IDisposable
{
    public required IntPtr WindowHandle { get; init; }
    public required  IntPtr ContextHandle { get; init; }

    public int BackBufferWidth
    {
        get
        {
            GetWindowSize(WindowHandle, out var width, out _);
            return width;
        }
    }

    public int BackBufferHeight
    {
        get
        {
            GetWindowSize(WindowHandle, out _, out var height);
            return height;
        }
    }

    public static nint GetProcAddress(string name) => GLGetProcAddress(name);
    public void MakeCurrent() => GLMakeCurrent(WindowHandle, ContextHandle).LogErrorIfFailed();
    public void SwapBuffers() => GLSwapWindow(WindowHandle).LogErrorIfFailed();

    public void Dispose()
    {
        GLDestroyContext(ContextHandle);
    }
}
