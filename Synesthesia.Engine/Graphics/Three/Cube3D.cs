// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Resources;
using Synesthesia.Utils.Extensions;

namespace Synesthesia.Engine.Graphics.Three;

public class Cube3D : Drawable3D
{

    [Singleton]
    private IResourceStore<Texture> textureStore = null!;

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

    private static readonly (Vector3 n, Vector3 r, Vector3 u)[] faces =
    [
        (new Vector3( 0,  0,  1), new Vector3( 1, 0,  0), new Vector3(0, 1,  0)), // +Z
        (new Vector3( 0,  0, -1), new Vector3(-1, 0,  0), new Vector3(0, 1,  0)), // -Z
        (new Vector3( 1,  0,  0), new Vector3( 0, 0, -1), new Vector3(0, 1,  0)), // +X
        (new Vector3(-1,  0,  0), new Vector3( 0, 0,  1), new Vector3(0, 1,  0)), // -X
        (new Vector3( 0,  1,  0), new Vector3( 1, 0,  0), new Vector3(0, 0, -1)), // +Y
        (new Vector3( 0, -1,  0), new Vector3( 1, 0,  0), new Vector3(0, 0,  1)), // -Y
    ];

    protected override void OnDraw3D()
    {
        Renderer.GraphicsDevice.BindTexture(textureStore.Get("Synesthesia.Resources.Textures.cat.png"));
        var batch = Renderer.VertexBatch;
        var half = Size * 0.5f;
        foreach (var (n, r, u) in faces)
        {
            batch.PushVertex(new Vertex3D((n - r - u) * half, n, r, packedColor, new Vector2(0, 1)));
            batch.PushVertex(new Vertex3D((n + r - u) * half, n, r, packedColor, new Vector2(1, 1)));
            batch.PushVertex(new Vertex3D((n + r + u) * half, n, r, packedColor, new Vector2(1, 0)));
            batch.PushVertex(new Vertex3D((n - r + u) * half, n, r, packedColor, new Vector2(0, 0)));
        }
    }

    protected override void OnLayout(Invalidation dirty)
    {
        base.OnLayout(dirty);

        if (dirty.HasFlagFast(Invalidation.DrawNode))
        {
            packedColor = Color.ToRgba32();
        }
    }
}
