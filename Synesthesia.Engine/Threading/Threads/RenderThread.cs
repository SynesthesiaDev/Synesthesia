// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Graphics.Two;
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

    private CompositeDrawable2d mainComposite = null!;

    [Singleton]
    private IResourceStore<Texture> textureResourceStore = null!;

    [Singleton]
    private IResourceStore<Font> fontResourceStore = null!;

    protected override void OnThreadInit()
    {
        Renderer.Surface.ClaimOwnership();
        Logger.Verbose("Transferred renderer context ownership to Render Thread", Logger.Platform);
        hasContextOwnership = true;

        (textureResourceStore as DeferredStore<Texture>)?.Unlock();
        (fontResourceStore as DeferredStore<Font>)?.Unlock();

        Renderer.CompileDefaultShaders();

        var font = fontResourceStore.Get("Synesthesia.Resources.Fonts.Quicksand-Regular.ttf");

        mainComposite = new CompositeDrawable2d
        {
            Size = new Vector2(Renderer.Surface.BackBufferWidth, Renderer.Surface.BackBufferHeight),
            Parent = null,
            Children =
            [
                new Text2d
                {
                    Text = "Testing testing",
                },
                // new Box2d
                // {
                //     Anchor = Anchor.Centre,
                //     Origin = Anchor.Centre,
                //     Texture = font.Atlas.TextureAtlas.AtlasTexture,
                //     Size = new Vector2(200, 200),
                //     TextureFillMode = TextureFillMode.Fit
                // },
                // new FillFlowContainer
                // {
                //     AutoSizeAxes = Axes.Both,
                //     Direction = Direction.Vertical,
                //     Anchor = Anchor.Centre,
                //     Origin = Anchor.Centre,
                //     Children =
                //     [
                //         new Box2d
                //         {
                //             Size = new Vector2(50, 50),
                //             Anchor = Anchor.TopLeft,
                //             Origin = Anchor.TopLeft,
                //         },
                //         new Circle2d
                //         {
                //             Size = new Vector2(50, 50),
                //             Anchor = Anchor.TopLeft,
                //             Origin = Anchor.TopLeft,
                //         },
                //         new Box2d
                //         {
                //             Size = new Vector2(50, 50),
                //             Anchor = Anchor.TopLeft,
                //             Origin = Anchor.TopLeft,
                //             Color = Color.ForestGreen,
                //             Texture = textureResourceStore.Get("Synesthesia.Resources.Textures.dull_blade.png"),
                //         },
                //         new Circle2d
                //         {
                //             Size = new Vector2(50, 50),
                //             Anchor = Anchor.TopLeft,
                //             Origin = Anchor.TopLeft,
                //             Color = Color.Crimson,
                //         },
                //     ],
                // },
            ],
        };

        mainComposite.Load();
    }

    protected override void ProcessFrame(FrameInfo frameInfo)
    {
        if (!Renderer.CanDraw || !hasContextOwnership) return;

        Renderer.BeginDrawing();

        mainComposite.Size = new Vector2(Renderer.BackBufferWidth, Renderer.BackBufferHeight);

        mainComposite.OnDraw();

        Renderer.EndDrawing();
    }
}
