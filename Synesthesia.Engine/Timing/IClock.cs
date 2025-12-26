// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Timing
{
    public interface IClock
    {
        double CurrentTime { get; }

        double Rate { get; }

        bool IsRunning { get; }
    }
}