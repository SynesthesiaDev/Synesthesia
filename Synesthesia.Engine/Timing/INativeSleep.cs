// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Timing;

public interface INativeSleep : IDisposable
{
    bool Sleep(TimeSpan duration);
}
