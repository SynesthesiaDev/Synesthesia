// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Commands;
using Synesthesia.Engine.Graphics.Materials;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Resources;

namespace Synesthesia.Engine.Graphics.Three;

public class Skybox3D : Drawable3D
{
    [Singleton]
    private IResourceStore<Mesh> meshResourceStore = null!;

    [Singleton]
    private IResourceStore<Texture> textureResourceStore = null!;

    private Mesh? mesh;

    public Material? Material { get; set; }

    protected override void Collect(RenderQueue renderQueue)
    {
        if(mesh == null || Material == null) return;
        renderQueue.Submit(new DrawMeshCommand(mesh, Material ?? Renderer.GraphicsDevice.DefaultMaterial, DrawMatrix.Matrix));
    }

    protected override void OnLoading()
    {
        Material ??= SkyboxMaterial.From("sky", textureResourceStore.Get("Synesthesia.Resources.Textures.skybox_default.png"));
        mesh = meshResourceStore.Get("default_cube");
        base.OnLoading();
    }

    protected override void UpdateDrawMatrix()
    {
        DrawMatrix.Reset();

        // if (Renderer.Camera != null)
        // {
        //     var camPos = Renderer.Camera.Position;
        //     DrawMatrix.Translate(camPos.X, camPos.Y, camPos.Z);
        // }

        DrawMatrix.Scale(1000f, 1000f, 1000f);
        // DrawMatrix.Scale(Width * Scale.X, Height * Scale.Y, Length * Scale.Z);
        // DrawMatrix.Rotate(RawRotation);
    }
}
