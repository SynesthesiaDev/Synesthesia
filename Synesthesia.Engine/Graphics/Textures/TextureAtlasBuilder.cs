// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.Numerics;
using RectpackSharp;
using Silk.NET.OpenGL;

namespace Synesthesia.Engine.Graphics.Textures;

public class TextureAtlasBuilder
{
    private const int padding = 2;

    private class PendingItem(int id, int width, int height, byte[] rgbaData)
    {
        public readonly int Id = id;
        public readonly int Width = width;
        public readonly int Height = height;
        public readonly byte[] RgbaData = rgbaData;
    }

    private readonly List<PendingItem> pendingItems = [];

    public void Add(int id, int width, int height, byte[] rgbaData)
    {
        pendingItems.Add(new PendingItem(id, width, height, rgbaData));
    }

    public void Add(int id, Texture texture)
    {
        if (texture.Data == null) throw new InvalidOperationException("Cannot add empty texture to texture atlas!");
        pendingItems.Add(new PendingItem(id, texture.Width, texture.Height, texture.Data!));
    }

    public TextureAtlas Build()
    {
        var rectangles = pendingItems.Select(i =>
            new PackingRectangle(0, 0, (uint)(i.Width + padding), (uint)(i.Height + padding), i.Id)
        ).ToArray();

        RectanglePacker.Pack(rectangles, out var bounds);

        int atlasWidth = (int)bounds.Width;
        int atlasHeight = (int)bounds.Height;

        var atlasData = new byte[atlasWidth * atlasHeight * 4];
        var regionMapping = new Dictionary<int, TextureRegion>();

        foreach (var rect in rectangles)
        {
            var item = pendingItems.First(p => p.Id == rect.Id);

            blit(item.RgbaData, item.Width, item.Height, atlasData, (int)rect.X, (int)rect.Y, atlasWidth);
        }

        var texture = new Texture(atlasWidth, atlasHeight, atlasData, PixelFormat.Rgba, "TextureAtlas", true);

        foreach (var rect in rectangles)
        {
            var item = pendingItems.First(p => p.Id == rect.Id);

            var uv = new RectangleF(
                (float)rect.X / atlasWidth,
                (float)rect.Y / atlasHeight,
                (float)item.Width / atlasWidth,
                (float)item.Height / atlasHeight
            );

            regionMapping[item.Id] = new TextureRegion(texture, uv, new Vector2(item.Width, item.Height));
        }

        return new TextureAtlas(atlasWidth, atlasHeight, texture, regionMapping);
    }

    private static void blit(byte[] src, int sw, int sh, byte[] dst, int dx, int dy, int dw)
    {
        for (int y = 0; y < sh; y++)
        {
            int srcOffset = y * sw * 4;
            int dstOffset = ((dy + y) * dw + dx) * 4;

            if (dstOffset + (sw * 4) > dst.Length)
            {
                continue;
            }

            Array.Copy(src, srcOffset, dst, dstOffset, sw * 4);
        }
    }
}
