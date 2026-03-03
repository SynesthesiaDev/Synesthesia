using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Graphics.Textures;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Shapes;

public class Box2d : ColoredDrawable2d
{
    public float CornerRadius { get; set; } = 0f;

    public Texture? Texture = null;

    private Rectangle textureRect = new(0, 0, 0, 0);
    private Rectangle boxRect = new(0, 0, 0, 0);

    private TextureFillMode textureFillMode = TextureFillMode.Stretch;

    public TextureFillMode TextureFillMode
    {
        get => textureFillMode;
        set
        {
            if (value == textureFillMode) return;
            textureFillMode = value;
            Invalidate(Invalidation.DrawNode);
        }
    }

    protected override void OnLayout(Invalidation dirty)
    {
        base.OnLayout(dirty);

        if (!(dirty.HasFlagFast(Invalidation.Geometry) | dirty.HasFlagFast(Invalidation.DrawNode))) return;

        boxRect = new Rectangle(Vector2.Zero, Size);
        if (Texture is not { IsUploaded: true })
        {
            textureRect = new Rectangle(Vector2.Zero, Vector2.Zero);
        }
        else
        {
            textureRect = Texture.Rect;

            if (TextureFillMode == TextureFillMode.Stretch) return;

            var textureRatio = Texture.Width.ToFloat() / Texture.Height;
            var boxRatio = Size.X / Size.Y;

            switch (TextureFillMode)
            {
                case TextureFillMode.Fit when textureRatio > boxRatio:
                {
                    var height = Size.X / textureRatio;
                    boxRect = new Rectangle(0, (Size.Y - height) / 2f, Size.X, height);
                    break;
                }
                case TextureFillMode.Fit:
                {
                    float width = Size.Y * textureRatio;
                    boxRect = new Rectangle((Size.X - width) / 2f, 0, width, Size.Y);
                    break;
                }
                case TextureFillMode.Fill when textureRatio > boxRatio:
                {
                    var visibleWidth = Texture.Height * boxRatio;
                    textureRect = new Rectangle((Texture.Width - visibleWidth) / 2f, 0, visibleWidth, Texture.Height);
                    break;
                }
                case TextureFillMode.Fill:
                {
                    var visibleHeight = Texture.Width / boxRatio;
                    textureRect = new Rectangle(0, (Texture.Height - visibleHeight) / 2f, Texture.Width, visibleHeight);
                    break;
                }
            }
        }
    }

    protected override void OnDraw2d()
    {
        if (Texture != null)
        {
            Raylib.DrawTexturePro(Texture.NativeTexture, textureRect, boxRect, Vector2.Zero, 0f, ApplyAlpha(Color));
        }
        else
        {
            if (CornerRadius <= 0)
            {
                Raylib.DrawRectangleV(Vector2.Zero, Size, ApplyAlpha(Color));
            }
            else
            {
                var roundness = Math.Clamp(CornerRadius * 2 / Math.Min(Size.X, Size.Y), 0f, 1f);
                Raylib.DrawRectangleRounded(new Rectangle(0, 0, Size.X, Size.Y), roundness, 32, ApplyAlpha(Color));
            }
        }
    }
}
