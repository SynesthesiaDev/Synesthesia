// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Silk.NET.OpenGL;

namespace Synesthesia.Engine.Graphics;

public class InstanceBuffer : IDisposable
{
    private readonly GL gl;
    private readonly uint vbo;
    private readonly int maxInstances;
    private readonly Matrix4x4[] matrixData;

    public uint Vbo => vbo;

    public int InstanceCount { get; private set; }

    public InstanceBuffer(GL gl, int maxInstances = 10000)
    {
        this.gl = gl;
        this.maxInstances = maxInstances;
        matrixData = new Matrix4x4[maxInstances];

        vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        unsafe
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(maxInstances * sizeof(Matrix4x4)), data: null, BufferUsageARB.StreamDraw);
        }

    }

    public void Upload(ReadOnlySpan<Matrix4x4> matrices)
    {
        InstanceCount = matrices.Length;
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        unsafe
        {
            fixed (Matrix4x4* ptr = matrices)
            {
                gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(InstanceCount * sizeof(Matrix4x4)), ptr);
            }
        }
    }

    public void Dispose()
    {

    }
}
