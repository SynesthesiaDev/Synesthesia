// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics.Materials;
using Synesthesia.Engine.Graphics.Three;

namespace Synesthesia.Engine.Graphics;

public class RenderBatch(Mesh mesh, Material material)
{
    public readonly Mesh Mesh = mesh;
    public readonly Material Material = material;

    public readonly List<Matrix4x4> Instances = new();
}
