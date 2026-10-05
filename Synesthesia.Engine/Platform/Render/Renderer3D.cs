// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Engine.Util;

namespace Synesthesia.Engine.Platform.Render;

public class Renderer3D(GraphicsDevice graphicsDevice) : IRenderer<Vertex3D>
{
    public VertexBatch<Vertex3D> VertexBatch { get; } = new VertexBatch<Vertex3D>(graphicsDevice.OpenGL, is2D: false);
    public GL OpenGL { get; } = graphicsDevice.OpenGL;

    public readonly GraphicsDevice GraphicsDevice = graphicsDevice;

    private readonly Stack<Matrix4x4> matrixStack = new();
    private Matrix4x4 projectionMatrix;
    private int transformMatrixShaderLocation;

    private Matrix4x4 perspectiveMatrix;

    public Camera? Camera { get; set; }

    //TODO u_lightDirection
    //TODO u_lightColor
    //TODO u_ambientLight

    public Matrix4x4 Matrix { get; private set; } = Matrix4x4.Identity;

    public void PushMatrix()
    {
        GraphicsDevice.EnsureInitialized();
        matrixStack.Push(Matrix);
    }

    public void PopMatrix()
    {
        GraphicsDevice.EnsureInitialized();

        if (matrixStack.Count == 0) throw new InvalidOperationException("Matrix stack is empty");

        Matrix = matrixStack.Pop();

        UpdateShaderMatrix();
    }

    public void Translate(float x, float y, float z)
    {
        GraphicsDevice.EnsureInitialized();

        Matrix = Matrix4x4.CreateTranslation(x, y, z) * Matrix;

        UpdateShaderMatrix();
    }

    public void Scale(float x, float y, float z)
    {
        GraphicsDevice.EnsureInitialized();
        Matrix = Matrix4x4.CreateScale(x, y, z) * Matrix;

        UpdateShaderMatrix();
    }

    public void Rotate(float degrees, float x, float y, float z)
    {
        GraphicsDevice.EnsureInitialized();

        var rads = degrees.ToRads();
        var axis = Vector3.Normalize(new Vector3(x, y, z));

        Matrix = Matrix4x4.CreateFromAxisAngle(axis, rads) * Matrix;

        UpdateShaderMatrix();
    }

    public void BeginDrawing()
    {
        //TODO invalidation
        updatePerspective();

        OpenGL.Enable(EnableCap.DepthTest);
        OpenGL.DepthFunc(DepthFunction.Less); //TODO or equal... figure out which better

        OpenGL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        OpenGL.Enable(EnableCap.CullFace);
        OpenGL.CullFace(TriangleFace.Back);
        OpenGL.FrontFace(FrontFaceDirection.CW);
    }

    public void EndDrawing()
    {
        OpenGL.Disable(EnableCap.DepthTest);
    }

    public void FlushVertexBatch()
    {
        VertexBatch.Flush();
    }

    public void UpdateShaderMatrix()
    {
    }

    public void CacheUniformLocations()
    {
    }

    private void updatePerspective()
    {
        perspectiveMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4f, (float)GraphicsDevice.BackBufferWidth / GraphicsDevice.BackBufferHeight, 0.1f, 1000f);
    }
}
