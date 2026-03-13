// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Threading.Threads;

public class InputThread : ThreadRunner
{
    public override ThreadType Type => ThreadType.Input;

    protected override Logger.LogCategory LoggerCategory => Logger.Input;

    [Resolved]
    private InputHandler inputHandler = null!;

    protected override void ProcessFrame(FrameInfo frameInfo)
    {
        inputHandler.ProcessFrame();
    }

    protected override void OnThreadInit()
    {
    }

}
