// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Graphics.Three;

public class Mesh(string name, Vertex3D[] vertices, uint[] indices) : INamedResource
{
    public uint Vao, Vbo, Ibo;
    public readonly int IndexCount = indices.Length;

    public ulong Handle => Vao;
    private GL gl = null!;

    public string ResourceName { get; } = name;
    public bool IsUploaded { get; private set; }

    public void Upload(GraphicsDevice graphicsDevice)
    {
        if(IsUploaded) return;
        ThreadSafety.AssertRunningOnRenderThread();

        gl = graphicsDevice.OpenGL;

        Vao = gl.GenVertexArray();
        gl.BindVertexArray(Vao);
        gl.CheckError("Generate and bind vao array");

        Vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo);
        gl.CheckError("Generate and bind vbo buffer");

        unsafe
        {
            fixed (Vertex3D* pointer = vertices)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(Vertex3D)), data: pointer, BufferUsageARB.StaticDraw);
        }

        Ibo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, Ibo);
        gl.CheckError("Generate and bind Ibo buffer");
        unsafe
        {
            fixed (uint* p = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                    (nuint)(indices.Length * sizeof(uint)),
                    p, BufferUsageARB.StaticDraw);
        }

        Reflection.SetupVertexAttributes<Vertex3D>(gl);
        gl.CheckError("Setup vertex attributes");

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, graphicsDevice.Renderer3D.InstanceBuffer.Vbo);
        unsafe
        {
            int stride = sizeof(Matrix4x4);
            for (int i = 0; i < 4; i++)
            {
                uint location = (uint)(5 + i);
                gl.EnableVertexAttribArray(location);

                gl.VertexAttribPointer(location, 4, VertexAttribPointerType.Float, false, (uint)stride, (void*)(i * 16));

                gl.VertexAttribDivisor(location, 1);
            }
        }

        gl.BindVertexArray(0);
        gl.CheckError("Unbind vao");
        IsUploaded = true;
        Logger.Verbose($"Uploaded {this}", Logger.Render);
    }

    public void Draw()
    {
        assertUploadedAndRenderThread();
        gl.BindVertexArray(Vao);
        unsafe
        {
            gl.DrawElements(PrimitiveType.Triangles,
                (uint)IndexCount,
                DrawElementsType.UnsignedInt,
                indices: null);
        }

        DrawStatistics.Increment(DrawStatistics.Type.DrawCalls3D);
    }

    public void DrawInstanced(int instanceCount)
    {
        assertUploadedAndRenderThread();
        gl.BindVertexArray(Vao);
        unsafe
        {
            gl.DrawElementsInstanced(PrimitiveType.Triangles,
                (uint)IndexCount,
                DrawElementsType.UnsignedInt,
                indices: null,
                (uint)instanceCount);
        }

        DrawStatistics.Increment(DrawStatistics.Type.DrawCalls3D);
    }

    public void Dispose()
    {
        if (IsUploaded)
        {
            gl.DeleteBuffer(Vbo);
            gl.DeleteBuffer(Ibo);
            gl.DeleteVertexArray(Vao);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void assertUploadedAndRenderThread()
    {
        ThreadSafety.AssertRunningOnRenderThread();
        if (IsUploaded) return;
        throw new InvalidOperationException("Mesh has not been uploaded yet");
    }

    public override string ToString()
    {
        return $"Mesh(Vertices={vertices.Length}, Indices={indices.Length}, Vao={Vao}, Vbo={Vbo}, Ibo={Ibo}, IsUploaded={IsUploaded})";
    }
}
