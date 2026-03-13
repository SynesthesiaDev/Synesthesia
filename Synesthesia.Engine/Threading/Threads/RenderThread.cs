// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Threading.Threads;

public class RenderThread(OpenGlRenderer renderer) : ThreadRunner
{
    public OpenGlRenderer Renderer { get; } = renderer;

    public override ThreadType Type => ThreadType.Draw;

    protected override Logger.LogCategory LoggerCategory => Logger.Render;

    private bool hasContextOwnership;

    protected override void OnThreadInit()
    {
        Renderer.Surface.ClaimOwnership();
        Logger.Verbose("Transferred renderer context ownership to Render Thread", Logger.Platform);
        hasContextOwnership = true;
    }

    protected override void ProcessFrame(FrameInfo frameInfo)
    {
        if(!Renderer.CanDraw || !hasContextOwnership) return;

        Renderer.BeginDrawing();
        Renderer.OpenGL.ClearColor(0.39f, 0.58f, 0.93f, 1.0f);
        Renderer.EndDrawing();
    }
}
