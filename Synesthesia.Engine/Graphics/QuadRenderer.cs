// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Silk.NET.OpenGL;
using Synesthesia.Engine.Extensions;

namespace Synesthesia.Engine.Graphics;

public class QuadRenderer : IDisposable
{
    private readonly GL gl;
    private uint vao;
    private uint vbo;

    public QuadRenderer(GL gl)
    {
        this.gl = gl;
        initializeBuffers();
    }

    private void initializeBuffers()
    {
        float[] vertices =
        {
            // X,   Y,   Z
            0f, 0f, 0f, // Top-Left
            0f, 1f, 0f, // Bottom-Left
            1f, 1f, 0f, // Bottom-Right

            1f, 1f, 0f, // Bottom-Right
            1f, 0f, 0f, // Top-Right
            0f, 0f, 0f  // Top-Left
        };

        gl.GenVertexArrays(1, out vao);
        gl.CheckError("Generate vao array");

        gl.BindVertexArray(vao);
        gl.CheckError("Bind vao array");

        gl.GenBuffers(1, out vbo);
        gl.CheckError("Generate vbo buffer");

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.CheckError("Bind vbo buffer");



        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        unsafe
        {
            fixed (float* v = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            }

            gl.CheckError("Set buffer data");

            gl.EnableVertexAttribArray(0);
            gl.CheckError("Enable vertex attrib array");

            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
            gl.CheckError("Vertex attrib pointer");

            gl.BindVertexArray(0);
            gl.CheckError("unbind vao");
        }
    }

    public void Draw()
    {
        gl.BindVertexArray(vao);
        gl.CheckError("Bind vao");
        gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        gl.CheckError("Draw arrays");
        gl.BindVertexArray(0);
        gl.CheckError("Unbind vao");
    }

    public void Dispose()
    {
        gl.DeleteVertexArray(vao);
        gl.DeleteBuffer(vbo);
    }
}
