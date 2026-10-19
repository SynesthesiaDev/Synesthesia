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

    private Matrix4x4 perspectiveMatrix;

    private int modelShaderLocation, viewProjShaderLocation, lightDirShaderLocation, lightColorShaderLocation, ambientShaderLocation;

    public Camera? Camera { get; set; }

    public Vector3 LightDirection
    {
        get;
        set => field = Vector3.Normalize(value);
    } = Vector3.Normalize(new Vector3(-0.4f, -1f, -0.3f));

    public Color LightColor { get; set; } = Color.White;
    public float AmbientLight { get; set; } = 0.2f;

    public Matrix4x4 Matrix
    {
        get;
        set
        {
            FlushVertexBatch();
            field = value;
            UpdateShaderMatrix();
        }
    } = Matrix4x4.Identity;

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
    }

    public void Translate(float x, float y, float z)
    {
        GraphicsDevice.EnsureInitialized();

        Matrix = Matrix4x4.CreateTranslation(x, y, z) * Matrix;
    }

    public void Scale(float x, float y, float z)
    {
        GraphicsDevice.EnsureInitialized();
        Matrix = Matrix4x4.CreateScale(x, y, z) * Matrix;
    }

    public void Rotate(float degrees, float x, float y, float z)
    {
        GraphicsDevice.EnsureInitialized();

        var rads = degrees.ToRads();
        var axis = Vector3.Normalize(new Vector3(x, y, z));

        Matrix = Matrix4x4.CreateFromAxisAngle(axis, rads) * Matrix;
    }

    public void BeginDrawing()
    {
        //TODO invalidation
        GraphicsDevice.BindShader(GraphicsDevice.DefaultShader3D);
        updatePerspective();

        OpenGL.Enable(EnableCap.DepthTest);
        OpenGL.DepthFunc(DepthFunction.Less); //TODO or equal... figure out which better

        OpenGL.Clear(ClearBufferMask.DepthBufferBit); // confused if I need to clear color buffer

        OpenGL.Disable(EnableCap.CullFace); //TODO enable culling
        // OpenGL.CullFace(TriangleFace.Back);
        // OpenGL.FrontFace(FrontFaceDirection.Ccw);

        uploadFrameUniforms();
        LoadIdentity();
    }

    public void EndDrawing()
    {
        FlushVertexBatch();
        OpenGL.Disable(EnableCap.CullFace);
        OpenGL.Disable(EnableCap.DepthTest);
        GraphicsDevice.UnbindShader();
    }

    public void FlushVertexBatch()
    {

        VertexBatch.Flush();
    }

    public void UpdateShaderMatrix()
    {
        GraphicsDevice.CurrentShader.SetMatrix4(modelShaderLocation, Matrix);
    }

    public void LoadIdentity()
    {
        GraphicsDevice.EnsureInitialized();
        Matrix = Matrix4x4.Identity;
    }

    public void CacheUniformLocations()
    {

        var shader = GraphicsDevice.CurrentShader;
        modelShaderLocation = shader.GetUniformLocation("u_model");
        viewProjShaderLocation = shader.GetUniformLocation("u_perspective");
        lightDirShaderLocation = shader.GetUniformLocation("u_lightDirection");
        lightColorShaderLocation = shader.GetUniformLocation("u_lightColor");
        ambientShaderLocation = shader.GetUniformLocation("u_ambientLight");
    }

    private void uploadFrameUniforms()
    {
        var view = Camera?.ViewMatrix ?? Matrix4x4.Identity;
        var viewProjection = view * perspectiveMatrix;

        var shader = GraphicsDevice.CurrentShader;
        shader.SetMatrix4(viewProjShaderLocation, viewProjection);
        shader.SetVector3(lightDirShaderLocation, LightDirection);
        shader.SetColor(lightColorShaderLocation, LightColor);
        shader.SetFloat(ambientShaderLocation, AmbientLight);
    }

    private void updatePerspective()
    {
        perspectiveMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4f, (float)GraphicsDevice.BackBufferWidth / GraphicsDevice.BackBufferHeight, 0.1f, 1000f);
    }
}
