// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Threading.Threads;

public class UpdateThread : ThreadRunner
{
    public override ThreadType Type => ThreadType.Update;

    protected override Logger.LogCategory LoggerCategory => Logger.Runtime;

    protected override void ProcessFrame(FrameInfo frameInfo)
    {
    }

    protected override void OnThreadInit()
    {
    }

}
