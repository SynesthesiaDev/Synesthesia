// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Graphics.Font;

public class FontHandle(Raylib_cs.Font font)
{
    public Raylib_cs.Font NativeFont => font;
}
