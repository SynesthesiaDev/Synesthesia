// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Three;
using System.Runtime.InteropServices;
using Shader = Synesthesia.Engine.Graphics.Shader;

namespace Synesthesia.Engine.Platform.Render;

public class Renderer3D(GraphicsDevice graphicsDevice) : IRenderer<Vertex3D> {

    public VertexBatch<Vertex3D> VertexBatch { get; } = new VertexBatch<Vertex3D>(graphicsDevice.OpenGL, is2D: false);
    public GL OpenGL { get; } = graphicsDevice.OpenGL;

    public readonly GraphicsDevice GraphicsDevice = graphicsDevice;

    private Matrix4x4 perspectiveMatrix;

    private int modelShaderLocation, viewProjShaderLocation, lightDirShaderLocation, lightColorShaderLocation, ambientShaderLocation;

    private Shader? lastShaderWithUniforms;
    public Camera? Camera { get; set; }

    public readonly RenderQueue RenderQueue = new RenderQueue();

    public Vector3 LightDirection
    {
        get;
        set => field = Vector3.Normalize(value);
    } = Vector3.Normalize(new Vector3(-0.4f, -1f, -0.3f));

    public Color LightColor { get; set; } = Color.White;
    public float AmbientLight { get; set; } = 0.2f;

    public void BeginDrawing()
    {
        RenderQueue.Clear();

        OpenGL.Enable(EnableCap.DepthTest);
        OpenGL.DepthFunc(DepthFunction.Less);

        OpenGL.Clear(ClearBufferMask.DepthBufferBit);

        OpenGL.Enable(EnableCap.CullFace);
        OpenGL.CullFace(TriangleFace.Back);
        OpenGL.FrontFace(FrontFaceDirection.Ccw);

        updatePerspective();
    }

    public void EndDrawing()
    {
        foreach (var renderBatch in RenderQueue.GetBatches())
        {
            GraphicsDevice.BindMaterial(renderBatch.Material);

            if (GraphicsDevice.CurrentShader != lastShaderWithUniforms)
            {
                uploadGlobalFrameUniforms();
                lastShaderWithUniforms = GraphicsDevice.CurrentShader;
            }

            foreach (ref Matrix4x4 matrix in CollectionsMarshal.AsSpan(renderBatch.Instances))
            {
                GraphicsDevice.CurrentShader.SetMatrix4(modelShaderLocation, matrix);
                renderBatch.Mesh.Draw();
            }
        }

        lastShaderWithUniforms = null;

        OpenGL.Disable(EnableCap.CullFace);
        OpenGL.Disable(EnableCap.DepthTest);
        GraphicsDevice.UnbindShader();
    }

    public void FlushVertexBatch()
    {
    }

    public void UpdateShaderMatrix()
    {
        //noop
    }

    public void CacheGlobalUniformLocations()
    {
        var shader = GraphicsDevice.CurrentShader;
        modelShaderLocation = shader.GetUniformLocation("u_model");
        viewProjShaderLocation = shader.GetUniformLocation("u_perspective");
        lightDirShaderLocation = shader.GetUniformLocation("u_lightDirection");
        lightColorShaderLocation = shader.GetUniformLocation("u_lightColor");
        ambientShaderLocation = shader.GetUniformLocation("u_ambientLight");
    }

    private void uploadGlobalFrameUniforms()
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
