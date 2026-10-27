// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;
using Synesthesia.Engine.Graphics.Materials;
using Synesthesia.Engine.Graphics.Three;

namespace Synesthesia.Engine.Graphics.Commands;

[StructLayout(LayoutKind.Auto)]
public readonly struct DrawMeshCommand(Mesh mesh, Material material, Matrix4x4 worldMatrix) : IRenderCommand
{
    public readonly Mesh Mesh = mesh;
    public readonly Material Material = material;
    public readonly Matrix4x4 WorldMatrix = worldMatrix;
}
