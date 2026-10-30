// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Materials;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Components.Three;

public class MicrowavablePaimonDrawable : CompositeDrawable3D
{

    [Singleton]
    private IResourceStore<Mesh> meshResourceStore = null!;

    [Singleton]
    private IResourceStore<Texture> textureResourceStore = null!;

    private MeshDrawable3D meshDrawable = null!;

    protected override void OnLoading()
    {
        var mesh = meshResourceStore.Get("Synesthesia.Resources.Meshes.paimon.obj");
        var paimonTexture = textureResourceStore.Get("Synesthesia.Resources.Textures.paimon.png");
        var paimonMaterial = DefaultMaterial.From(paimonTexture);

        Children =
        [
            meshDrawable = new MeshDrawable3D
            {
                Mesh = mesh,
                Material = paimonMaterial,
            },
        ];
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        base.OnUpdate(frameInfo);
        meshDrawable.Rotation += new Vector3(0, 50f * frameInfo.DeltaSeconds, 0);
    }
}
