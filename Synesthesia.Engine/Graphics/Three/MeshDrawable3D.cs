// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics.Commands;
using Synesthesia.Engine.Graphics.Materials;

namespace Synesthesia.Engine.Graphics.Three;

public class MeshDrawable3D : Drawable3D
{
    public Material? Material { get; set; }

    public Mesh? Mesh { get; set; }

    protected override void Collect(RenderQueue renderQueue)
    {
        if(Mesh == null) return;
        renderQueue.Submit(new DrawMeshCommand(Mesh, Material ?? Renderer.GraphicsDevice.DefaultMaterial, DrawMatrix.Matrix));
    }
}
