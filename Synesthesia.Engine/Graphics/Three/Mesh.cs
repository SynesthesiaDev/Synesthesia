// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Graphics.Three;

public class Mesh : INamedResource
{
    public readonly uint Vao, Vbo, Ibo;
    public readonly int IndexCount;

    public ulong Handle => Vao;
    private readonly GL gl;

    public string ResourceName { get; }
    public Mesh(string name, GraphicsDevice graphicsDevice, Vertex3D[] vertices, uint[] indices)
    {
        ResourceName = name;
        IndexCount = indices.Length;

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
    }

    public void Draw()
    {
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
        gl.DeleteBuffer(Vbo);
        gl.DeleteBuffer(Ibo);
        gl.DeleteVertexArray(Vao);
    }

}
