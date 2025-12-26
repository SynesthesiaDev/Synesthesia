using Common.Logger;
using Raylib_cs;

namespace Synesthesia.Engine.Utility;

public static unsafe class Unsafe
{
    public static unsafe Font LoadFontFromMemory(byte[] fontData)
    {
        return Raylib.LoadFontFromMemory(".ttf", fontData, 24, null, 0);
    }
}