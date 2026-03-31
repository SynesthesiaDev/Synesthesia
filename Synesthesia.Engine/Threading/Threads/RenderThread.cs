// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Resources.Stores;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Threading.Threads;

public class RenderThread(OpenGlRenderer renderer) : ThreadRunner
{
    public OpenGlRenderer Renderer { get; } = renderer;

    public override ThreadType Type => ThreadType.Draw;

    protected override Logger.LogCategory LoggerCategory => Logger.Render;

    private bool hasContextOwnership;

    [Singleton]
    private IResourceStore<Texture> textureResourceStore = null!;

    [Singleton]
    private IResourceStore<Font> fontResourceStore = null!;

    [Singleton]
    private Game game = null!;

    private bool isFirstSwap = true;

    protected override void OnThreadInit()
    {
        Renderer.Surface.ClaimOwnership();
        Logger.Verbose("Transferred renderer context ownership to Render Thread", Logger.Platform);
        hasContextOwnership = true;

        (textureResourceStore as DeferredResourceStore<Texture>)?.Unlock();
        (fontResourceStore as DeferredResourceStore<Font>)?.Unlock();

        Renderer.CompileDefaultShaders();
        fontResourceStore.Get("Synesthesia.Resources.Fonts.Quicksand-Regular.fna"); // cache

        game.UpdateThread.Schedule(() =>
        {
            game.GetInternalGameContainer().Load();
            LoadFuture.Complete(this);
        });
    }

    protected override void ProcessFrame(FrameInfo frameInfo)
    {
        if (!Renderer.CanDraw || !hasContextOwnership) return;
        var gameContainer = game.GetInternalGameContainer();

        Renderer.BeginDrawing();

        gameContainer.OnDraw();

        Renderer.EndDrawing();
        if (isFirstSwap)
        {
            isFirstSwap = false;
            game.WindowHost.Show();
        }
    }
}
