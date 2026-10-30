// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;
using Synesthesia.Utils.Extensions;

namespace Synesthesia.Parsers.Model;

public record MeshTemplate(List<MeshTemplate.Vertex> Vertices, List<uint> Indices) : IResourceTemplate
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vertex(
        Vector3 position,
        Vector3 normal,
        Vector2 textureCoord)
    {
        public readonly Vector3 Position = position;
        public readonly Vector3 Normal = normal;
        public readonly Vector2 TextureCoord = textureCoord;

        public override string ToString()
        {
            return $"MeshTemplate.Vertex(Position={Position}, Normal={Normal}, TextureCoord={TextureCoord})";
        }
    }

    public override string ToString()
    {
        return $"MeshTemplate(Vertices={Vertices.ToListString()}, Indices={Indices.ToListString()})";
    }
}
