// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Threading.Threads;

public class UpdateThread : ThreadRunner
{
    public override ThreadType Type => ThreadType.Update;

    protected override Logger.LogCategory LoggerCategory => Logger.Runtime;

    [Singleton]
    private Game game = null!;

    [Singleton]
    private OpenGlRenderer renderer = null!;

    protected override void ProcessFrame(FrameInfo frameInfo)
    {
        game.DrawableScene2d.Size = new Vector2(renderer.BackBufferWidth, renderer.BackBufferHeight);
        game.DrawableScene2d.OnUpdate(frameInfo);
    }

    protected override void OnThreadInit()
    {
    }
}
