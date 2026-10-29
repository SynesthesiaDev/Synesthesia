// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Three;
using System.Runtime.InteropServices;
using Shader = Synesthesia.Engine.Graphics.Shader;

namespace Synesthesia.Engine.Platform.Render;

public class Renderer3D : IRenderer<Vertex3D>
{
    public VertexBatch<Vertex3D> VertexBatch { get; }
    public GL OpenGL { get; }

    public readonly GraphicsDevice GraphicsDevice;

    private int viewProjShaderLocation, lightDirShaderLocation, lightColorShaderLocation, ambientShaderLocation;

    private Shader? lastShaderWithUniforms;
    public Camera? Camera { get; set; }

    public readonly InstanceBuffer InstanceBuffer;

    public readonly RenderQueue RenderQueue = new RenderQueue();

    public Renderer3D(GraphicsDevice graphicsDevice)
    {
        VertexBatch = new VertexBatch<Vertex3D>(graphicsDevice.OpenGL, is2D: false);
        OpenGL = graphicsDevice.OpenGL;
        GraphicsDevice = graphicsDevice;
        InstanceBuffer = new InstanceBuffer(GraphicsDevice);
    }

    public Vector3 LightDirection
    {
        get;
        set => field = Vector3.Normalize(value);
    } = Vector3.Normalize(new Vector3(-0.4f, -1f, -0.3f));

    public Color LightColor { get; set; } = Color.White;
    public float AmbientLight { get; set; } = 0.5f;

    public void BeginDrawing()
    {
        RenderQueue.Clear();

        OpenGL.Enable(EnableCap.DepthTest);
        OpenGL.DepthFunc(DepthFunction.Less);

        OpenGL.Clear(ClearBufferMask.DepthBufferBit);

        OpenGL.Enable(EnableCap.CullFace);
        OpenGL.CullFace(TriangleFace.Back);
        OpenGL.FrontFace(FrontFaceDirection.Ccw);

        // updatePerspective();
    }

    public void EndDrawing()
    {
        foreach (ref RenderBatch renderBatch in CollectionsMarshal.AsSpan(RenderQueue.GetBatches()))
        {
            if(!renderBatch.Mesh.IsUploaded) renderBatch.Mesh.Upload(GraphicsDevice);

            GraphicsDevice.BindMaterial(renderBatch.Material);

            if (GraphicsDevice.CurrentShader != lastShaderWithUniforms)
            {
                uploadGlobalFrameUniforms();
                lastShaderWithUniforms = GraphicsDevice.CurrentShader;
            }

            InstanceBuffer.Upload(CollectionsMarshal.AsSpan(renderBatch.Instances));
            GraphicsDevice.OpenGL.BindVertexArray(renderBatch.Mesh.Vao);
            renderBatch.Mesh.DrawInstanced(InstanceBuffer.InstanceCount);
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
        viewProjShaderLocation = shader.GetUniformLocation("u_perspective");
        lightDirShaderLocation = shader.GetUniformLocation("u_lightDirection");
        lightColorShaderLocation = shader.GetUniformLocation("u_lightColor");
        ambientShaderLocation = shader.GetUniformLocation("u_ambientLight");
    }

    private void uploadGlobalFrameUniforms()
    {
        var shader = GraphicsDevice.CurrentShader;

        if (Camera != null)
        {
            var aspectRation = (float)GraphicsDevice.BackBufferWidth / GraphicsDevice.BackBufferHeight;
            var fovRad = Camera.Fov * (MathF.PI / 180f);

            var projection = Matrix4x4.CreatePerspectiveFieldOfView(fovRad, aspectRation, Camera.ZNear, Camera.ZFar);

            var viewProjection = Camera.ViewMatrix * projection;
            shader.SetMatrix4(viewProjShaderLocation, viewProjection);
        }
        else
        {
            shader.SetMatrix4(viewProjShaderLocation, Matrix4x4.Identity);
        }

        shader.SetVector3(lightDirShaderLocation, LightDirection);
        shader.SetColor(lightColorShaderLocation, LightColor);
        shader.SetFloat(ambientShaderLocation, AmbientLight);
    }

    // private void updatePerspective()
    // {
    //     perspectiveMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4f, (float)GraphicsDevice.BackBufferWidth / GraphicsDevice.BackBufferHeight, 0.1f, 1000f);
    // }
}
