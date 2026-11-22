// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics.Materials;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Engine.Util.Pooling;
using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Graphics;

public class RenderBatch : IPooledObject
{
    public Mesh Mesh { get; set; } = null!;
    public Material Material { get; set; } = null!;

    public readonly List<Matrix4x4> Instances = [];

    public Vector3 Centroid
    {
        get
        {
            if (Instances.Count == 0) return Vector3.Zero;
            var sum = Vector3.Zero;
            foreach (ref Matrix4x4 m in CollectionsMarshal.AsSpan(Instances))
                sum += new Vector3(m.M41, m.M42, m.M43);
            return sum / Instances.Count;
        }
    }

    public void Reset()
    {
        Instances.Clear();
    }

    public bool IsPooled { get; set; }

    public Action<IPooledObject>? ReturnAction { get; set; }
}
