// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Statistics;
using Raylib_cs;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Threading.Runners;

namespace Synesthesia.Engine.Graphics.Textures;

public class Texture : IDisposable
{
    private Texture2D? nativeTexture;
    public Texture2D NativeTexture
    {
        get
        {
            ensureUploaded();
            return nativeTexture!.Value;
        }
    }

    private Image? pendingImage;

    public bool IsUploaded => nativeTexture.HasValue;

    private readonly RenderThread renderThread;

    public Rectangle Rect;

    public int Width => nativeTexture?.Width ?? 0;

    public int Height => nativeTexture?.Height ?? 0;

    public TextureWrap Wrap
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            renderThread.Schedule(() => Raylib.SetTextureWrap(NativeTexture, field));
        }
    }

    public TextureFilter Filter
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            renderThread.Schedule(() => Raylib.SetTextureFilter(NativeTexture, field));
        }
    }

    public Texture(Image pendingImage)
    {
        renderThread = DependencyContainer.Get<RenderThread>();
        this.pendingImage = pendingImage;

        Filter = TextureFilter.Bilinear;
        Wrap = TextureWrap.Clamp;
    }

    public Texture(Texture2D nativeTexture)
    {
        renderThread = DependencyContainer.Get<RenderThread>();

        this.nativeTexture = nativeTexture;
        Rect = new Rectangle(0, 0, Width, Height);
        Filter = TextureFilter.Bilinear;
        Wrap = TextureWrap.Clamp;
    }

    private void ensureUploaded()
    {
        if (IsUploaded || pendingImage == null) return;

        nativeTexture = Raylib.LoadTextureFromImage(pendingImage.Value);
        Raylib.UnloadImage(pendingImage.Value);
        pendingImage = null;
        Rect = new Rectangle(0, 0, Width, Height);
        EngineStatistics.TEXTURE_UPLOADS.Increment();

        Raylib.SetTextureFilter(NativeTexture, Filter);
        Raylib.SetTextureWrap(NativeTexture, Wrap);
    }

    public void Dispose()
    {
        if (IsUploaded)
        {
            Raylib.UnloadTexture(NativeTexture);
        }
    }
}
