// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Commands;
using Synesthesia.Engine.Graphics.Materials;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Resources;

namespace Synesthesia.Engine.Graphics.Three;

public class Cube3D : Drawable3D
{
    [Singleton]
    private IResourceStore<Mesh> meshStore = null!;

    public Material? Material { get; set; }

    protected override void Collect(RenderQueue renderQueue)
    {
        if(mesh == null) return;
        renderQueue.Submit(new DrawMeshCommand(mesh, Material ?? Renderer.GraphicsDevice.DefaultMaterial, DrawMatrix.Matrix));
    }

    private Mesh? mesh;

    protected override void OnLoading()
    {
        mesh = meshStore.Get("default_cube");
        base.OnLoading();
    }

    public static Mesh CreateSharedMesh(GraphicsDevice graphics)
    {
        Vector3[] faceNormals =
        [
            new(0, 0, 1), new(0, 0, -1),
            new(1, 0, 0), new(-1, 0, 0),
            new(0, 1, 0), new(0, -1, 0),
        ];

        var vertices = new List<Vertex3D>(24);
        var indices = new List<uint>(36);

        foreach (var n in faceNormals)
        {
            var t = Vector3.Cross(n, Vector3.UnitY);
            if (t.LengthSquared() < 0.001f) t = Vector3.Cross(n, Vector3.UnitX);
            t = Vector3.Normalize(t);
            var b = Vector3.Cross(n, t);

            int baseIndex = vertices.Count;
            vertices.Add(new Vertex3D(n - t - b,  n, t, 0xFFFFFFFF, new Vector2(0, 1)));
            vertices.Add(new Vertex3D(n + t - b,  n, t, 0xFFFFFFFF, new Vector2(1, 1)));
            vertices.Add(new Vertex3D(n + t + b,  n, t, 0xFFFFFFFF, new Vector2(1, 0)));
            vertices.Add(new Vertex3D(n - t + b,  n, t, 0xFFFFFFFF, new Vector2(0, 0)));

            // CCW
            indices.AddRange([
                (uint)(baseIndex + 0), (uint)(baseIndex + 1), (uint)(baseIndex + 2),
                (uint)(baseIndex + 0), (uint)(baseIndex + 2), (uint)(baseIndex + 3)
            ]);
        }

        return new Mesh("default_cube", graphics, [.. vertices], [.. indices]);
    }
}
