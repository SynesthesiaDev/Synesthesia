// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Extensions;

namespace Synesthesia.Engine.Graphics;

public class QuadRenderer : IDisposable
{
    private readonly GL gl;
    private uint vao;
    private uint vbo;
    private uint ebo;

    public QuadRenderer(GL gl)
    {
        this.gl = gl;
        initializeBuffers();
    }

    private void initializeBuffers()
    {
        Vertex2d[] vertices =
        [
            new Vertex2d(new Vector2(0f, 0f), new Vector2(0f, 0f), Color.White),
            new Vertex2d(new Vector2(0f, 1f), new Vector2(0f, 1f), Color.White),
            new Vertex2d(new Vector2(1f, 1f), new Vector2(1f, 1f), Color.White),
            new Vertex2d(new Vector2(1f, 0f), new Vector2(1f, 0f), Color.White),
        ];

        uint[] indices =
        [
            0, 1, 2,
            2, 3, 0,
        ];

        gl.GenVertexArrays(1, out vao);
        gl.CheckError("Generate vao array");

        gl.BindVertexArray(vao);
        gl.CheckError("Bind vao array");

        gl.GenBuffers(1, out vbo);
        gl.CheckError("Generate vbo buffer");

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.CheckError("Bind vbo buffer");

        gl.GenBuffers(1, out ebo);
        gl.CheckError("Generate ebo buffer");

        unsafe
        {
            fixed (Vertex2d* v = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(Vertex2d)), v, BufferUsageARB.StaticDraw);
            }
            gl.CheckError("Set vbo data");

            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            gl.CheckError("Bind ebo");
            fixed (uint* i = indices)
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
            }
            gl.CheckError("Set ebo data");

            uint stride = (uint)sizeof(Vertex2d);

            gl.EnableVertexAttribArray(0);
            gl.CheckError("Enable vertex attrib array");

            // Position
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*)0);
            gl.CheckError("Set position size");

            // Texture Coordinates
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(2 * sizeof(float)));
            gl.CheckError("Set texture coordinates size");

            // Color
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, (void*)(4 * sizeof(float)));
            gl.CheckError("Set color size");

            gl.BindVertexArray(0);
            gl.CheckError("unbind vao");
        }
    }

    public void Draw()
    {
        gl.BindVertexArray(vao);
        gl.CheckError("Bind vao");

        unsafe
        {
            gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
        }
        gl.CheckError("Draw elements");

        gl.BindVertexArray(0);
        gl.CheckError("Unbind vao");
    }

    public void Dispose()
    {
        gl.DeleteVertexArray(vao);
        gl.DeleteBuffer(vbo);
        gl.DeleteBuffer(ebo);
    }
}
