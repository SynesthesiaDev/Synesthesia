// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using System.Numerics;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Parsers.Model;

namespace Synesthesia.Engine.Extensions;

public static class MeshTemplateExtensions
{
    public static Mesh ToMesh(this MeshTemplate template, string name)
    {
        List<Vertex3D> vertices = [.. template.Vertices.Select(templateVertex => new Vertex3D(templateVertex.Position, templateVertex.Normal, Vector3.Zero, 0xFFFFFF, templateVertex.TextureCoord))];
        return new Mesh(name, [.. vertices], [.. template.Indices]);
    }
}
