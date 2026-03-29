// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.Numerics;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Platform.Render;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two;

public class Box2d : Drawable2d
{
    [Singleton]
    private OpenGlRenderer renderer = null!;

    private RectangleF uvCoords = new(0, 0, 1, 1);
    private Vector2 drawSize;
    private Vector2 drawOffset;

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

        if (dirty.HasFlagFast(Invalidation.DrawNode))
        {
            packedColor = Color.ToRgba32();
        }

        if (dirty.HasFlagFast(Invalidation.Geometry))
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
                        {
                            scaleX = boxRatio / textureRatio;
                        }
                        else
                        {
                            scaleY = textureRatio / boxRatio;
                        }

                        uvCoords = new RectangleF((1f - scaleX) / 2f, (1f - scaleY) / 2f, scaleX, scaleY);
                        break;
                }
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

    protected override void OnDraw2d()
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
            textureCoord: uvCoords
        );
    }
}
