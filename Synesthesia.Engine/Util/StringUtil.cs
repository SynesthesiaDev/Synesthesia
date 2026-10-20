// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Util;

public static class StringUtil
{
    public static string FileName(string path) => string.Join('.', path.Split('.').TakeLast(2));
}
