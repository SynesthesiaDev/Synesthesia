// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.Numerics;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Util.Statistics;
using Synesthesia.Utils.Extensions;
using Texture = Synesthesia.Engine.Graphics.Textures.Texture;

namespace Synesthesia.Engine.Graphics.Two;

public class Box2D : Drawable2D
{
    [Singleton]
    private Renderer2D renderer = null!;

    private RectangleF uvCoords = new(0, 0, 1, 1);
    private Vector2 drawSize;
    private Vector2 drawOffset;
    private Framebuffer? ssFramebuffer;
    private Texture? ssTexture;
    private Vector2 cachedTargetSize;
    private bool supersampleDirty = true;

    public float CornerRadius { get; set; }

    public Texture? Texture
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.DrawNode);
        }
    } = null;

    public TextureFillMode TextureFillMode
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.DrawNode);
        }
    } = TextureFillMode.Stretch;

    public TextureFilterMode TextureFilterMode
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.DrawNode);
        }
    } = TextureFilterMode.Linear;

    public int Supersampling { get; set; } = 1;

    public Color Color
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.DrawNode);
        }
    } = Color.White;

    private uint packedColor;

    protected override void OnLayout(Invalidation dirty)
    {
        base.OnLayout(dirty);

        if (dirty.HasFlagFast(Invalidation.Geometry) || dirty.HasFlagFast(Invalidation.DrawNode))
        {
            recomputeDrawSize();
        }

        if (dirty.HasFlagFast(Invalidation.DrawNode) || dirty.HasFlagFast(Invalidation.Size))
        {
            if (dirty.HasFlagFast(Invalidation.DrawNode))
                packedColor = Color.ToRgba32();

            supersampleDirty = true;
        }
    }

    private void recomputeDrawSize()
    {
        drawSize = Size;
        drawOffset = Vector2.Zero;
        uvCoords = new RectangleF(0, 0, 1, 1);

        if (Texture != null && TextureFillMode != TextureFillMode.Stretch)
        {
            var textureRatio = (float)Texture.Width / Texture.Height;
            var boxRatio = Size.X / Size.Y;

            switch (TextureFillMode)
            {
                case TextureFillMode.Fit:
                    if (textureRatio > boxRatio)
                    {
                        drawSize = new Vector2(Size.X, Size.X / textureRatio);
                        drawOffset = new Vector2(0, (Size.Y - drawSize.Y) / 2f);
                    }
                    else
                    {
                        drawSize = new Vector2(Size.Y * textureRatio, Size.Y);
                        drawOffset = new Vector2((Size.X - drawSize.X) / 2f, 0);
                    }
                    break;

                case TextureFillMode.Fill:
                    var scaleX = 1f;
                    var scaleY = 1f;

                    if (textureRatio > boxRatio)
                        scaleX = boxRatio / textureRatio;
                    else
                        scaleY = textureRatio / boxRatio;

                    uvCoords = new RectangleF((1f - scaleX) / 2f, (1f - scaleY) / 2f, scaleX, scaleY);
                    break;
            }
        }
    }

    public Animation<float> ChangeCornerRadius(float newCornerRadius, long duration, Easing easing)
    {
        return TransformTo(nameof(CornerRadius), CornerRadius, newCornerRadius, duration, easing, Transforms.FLOAT, color => CornerRadius = color);
    }

    public Animation<Color> FadeColorTo(Color newColor, long duration, Easing easing)
    {
        return TransformTo(nameof(Color), Color, newColor, duration, easing, Transforms.COLOR, color => Color = color);
    }

    public Animation<Color> FlashBackground(Color flashColor, long durationIn, long durationOut, Easing easingIn, Easing easingOut)
    {
        var originalColor = Color;
        return TransformTo(nameof(Color), Color, flashColor, durationIn, easingIn, Transforms.COLOR, color => Color = color).Then(() =>
        {
            TransformTo(nameof(Color), flashColor, originalColor, durationOut, easingOut, Transforms.COLOR, color => Color = color);
        });
    }

    protected override void OnDraw2d()
    {
        if (Supersampling <= 1)
        {
            renderer.DrawQuad(
                drawMatrix: DrawMatrix,
                position: drawOffset,
                size: drawSize,
                packedColor: packedColor,
                alpha: InheritedAlpha,
                borderThickness: BorderThickness,
                borderHasSingleColor: BorderColor.HasSingleColor,
                borderColor: CachedBorderColor,
                cornerRadius: Math.Clamp(CornerRadius, 0f, Math.Min(Width, Height) / 2f),
                texture: Texture,
                textureCoord: uvCoords,
                filterMode: TextureFilterMode
            );

            return;
        }

        var targetSize = new Vector2(
            MathF.Max(1, MathF.Round(drawSize.X * Supersampling)),
            MathF.Max(1, MathF.Round(drawSize.Y * Supersampling)));


        if (ssFramebuffer == null || cachedTargetSize != targetSize)
        {
            ssTexture?.Dispose();

            ssFramebuffer = renderer.GraphicsDevice.CreateFramebuffer(targetSize);

            ssTexture = Texture.FromExistingHandle(renderer.GraphicsDevice.OpenGL, ssFramebuffer.Value.ColorTexture, (int)targetSize.X, (int)targetSize.Y, PixelFormat.Rgba);
            cachedTargetSize = targetSize;
            supersampleDirty = true;
        }

        if (supersampleDirty)
        {
            renderer.BeginRenderTarget(ssFramebuffer.Value);
            renderer.ClearCurrentTarget();


            renderer.DrawQuad(
                drawMatrix: DrawMatrix.IDENTITY, position: Vector2.Zero, size: targetSize,
                packedColor: packedColor, alpha: 1f,
                borderThickness: 0, borderHasSingleColor: true, borderColor: Matrix4x4.Identity,
                cornerRadius: 0, texture: Texture, textureCoord: uvCoords,
                filterMode: TextureFilterMode.Nearest
            );

            renderer.EndRenderTarget();
            ssTexture!.GenerateMipmaps();

            supersampleDirty = false;
            DrawStatistics.Increment(DrawStatistics.Type.SupersamplingDraws);
        }

        renderer.DrawQuad(
            drawMatrix: DrawMatrix, position: drawOffset, size: drawSize,
            packedColor: packedColor, alpha: InheritedAlpha,
            borderThickness: BorderThickness, borderHasSingleColor: BorderColor.HasSingleColor,
            borderColor: CachedBorderColor,
            cornerRadius: Math.Clamp(CornerRadius, 0f, Math.Min(Width, Height) / 2f),
            texture: ssTexture, textureCoord: new RectangleF(0, 1, 1, -1),
            filterMode: TextureFilterMode
        );
    }

    protected override void Dispose(bool isDisposing)
    {
        ssTexture?.Dispose();
        base.Dispose(isDisposing);
    }
}
