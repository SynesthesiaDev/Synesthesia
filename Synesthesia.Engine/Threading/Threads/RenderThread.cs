// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
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

    private CompositeDrawable2d mainComposite = null!;

    protected override void OnThreadInit()
    {
        Renderer.Surface.ClaimOwnership();
        Logger.Verbose("Transferred renderer context ownership to Render Thread", Logger.Platform);
        hasContextOwnership = true;

        Renderer.CompileDefaultShaders();

        mainComposite = new CompositeDrawable2d
        {
            Size = new Vector2(Renderer.Surface.BackBufferWidth, Renderer.Surface.BackBufferHeight),
            Parent = null,
            Children =
            [
                new Box2d
                {
                    Size = new Vector2(200, 200),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            ],
        };

        mainComposite.Load();
    }

    protected override void ProcessFrame(FrameInfo frameInfo)
    {
        if (!Renderer.CanDraw || !hasContextOwnership) return;

         // mainComposite.Size = new Vector2(Renderer.BackBufferWidth, Renderer.BackBufferHeight);

         Renderer.BeginDrawing();

         Renderer.PushMatrix();

         Renderer.Scale(100f, 100f, 1f);
         Renderer.QuadRenderer.Draw();

         Renderer.PopMatrix();

         Renderer.EndDrawing();
    }
}
