// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;

namespace Synesthesia.Parsers.Model;

public record MeshTemplate(MeshTemplate.Vertex[] Vertices, uint[] Indices) : IResourceTemplate
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
    }
}
