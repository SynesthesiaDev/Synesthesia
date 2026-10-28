// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Resources.Stores;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Threading.Threads;

public class RenderThread(GraphicsDevice graphicsDevice) : ThreadRunner
{
    public GraphicsDevice GraphicsDevice { get; } = graphicsDevice;

    public override ThreadType Type => ThreadType.Draw;

    protected override Logger.LogCategory LoggerCategory => Logger.Render;

    private bool hasContextOwnership;

    [Singleton]
    private IResourceStore<Texture> textureResourceStore = null!;

    [Singleton]
    private IResourceStore<Font> fontResourceStore = null!;

    [Singleton]
    private IResourceStore<Mesh> meshResourceStore = null!;

    [Singleton]
    private IResourceStore<Shader> shaderResourceStore = null!;

    [Singleton]
    private Game game = null!;

    private bool isFirstSwap = true;

    protected override void OnThreadInit()
    {
        GraphicsDevice.Surface.ClaimOwnership();
        Logger.Verbose("Transferred renderer context ownership to Render Thread", Logger.Platform);
        hasContextOwnership = true;


        (textureResourceStore as DeferredResourceStore<Texture>)?.UnlockAndResolvedGraphics(GraphicsDevice);
        (fontResourceStore as DeferredResourceStore<Font>)?.UnlockAndResolvedGraphics(GraphicsDevice);
        (meshResourceStore as DeferredResourceStore<Mesh>)?.UnlockAndResolvedGraphics(GraphicsDevice);
        (shaderResourceStore as DeferredResourceStore<Shader>)?.UnlockAndResolvedGraphics(GraphicsDevice);

        GraphicsDevice.CompileDefaultShaders();

        fontResourceStore.Get("Synesthesia.Resources.Fonts.Quicksand-Regular.fna"); // cache

        game.UpdateThread.Schedule(() =>
        {
            game.GetInternalGameContainer().Load();
            game.DrawableScene3D.Load();
            LoadFuture.Complete(this);
        });
    }

    protected override void ProcessFrame(FrameInfo frameInfo)
    {
        if (!GraphicsDevice.CanDraw || !hasContextOwnership) return;
        var gameContainer = game.GetInternalGameContainer();

        GraphicsDevice.BeginDrawing();

        GraphicsDevice.Renderer3D.BeginDrawing();
        game.DrawableScene3D.OnDraw();
        GraphicsDevice.Renderer3D.EndDrawing();

        GraphicsDevice.Renderer2D.BeginDrawing();

        gameContainer.OnDraw();

        GraphicsDevice.Renderer2D.EndDrawing();
        GraphicsDevice.EndDrawing();

        if (isFirstSwap)
        {
            isFirstSwap = false;
            game.WindowHost.Show();
        }
    }
}
