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
    private GraphicsDevice renderer = null!;

    protected override void ProcessFrame(FrameInfo frameInfo)
    {
        var windowSize = new Vector2(renderer.BackBufferWidth, renderer.BackBufferHeight);
        var gameContainer = game.GetInternalGameContainer();

        gameContainer.Size = windowSize;
        gameContainer.OnUpdate(frameInfo);
    }

    protected override void OnThreadInit()
    {
        LoadFuture.Complete(this);
    }
}
