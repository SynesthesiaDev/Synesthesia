// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Util;
using Framebuffer = Synesthesia.Engine.Graphics.Framebuffer;
using Texture = Synesthesia.Engine.Graphics.Textures.Texture;

namespace Synesthesia.Engine.Platform.Render;

public class Renderer2D(GraphicsDevice graphicsDevice) : IRenderer<Vertex2D>
{
    private const string shader_uniform_transform_matrix = "u_transform";

    public readonly GraphicsDevice GraphicsDevice = graphicsDevice;

    private readonly Stack<Matrix4x4> matrixStack = new();
    private readonly Stack<Matrix4x4> inverseMatrixStack = new();
    private Matrix4x4 projectionMatrix;
    private int transformMatrixShaderLocation;

    public VertexBatch<Vertex2D> VertexBatch { get; private set; } = new(graphicsDevice.OpenGL, true);

    public Matrix4x4 Matrix { get; private set; } = Matrix4x4.Identity;
    public Matrix4x4 InverseMatrix { get; private set; } = Matrix4x4.Identity;

    public void PushMatrix()
    {
        GraphicsDevice.EnsureInitialized();
        matrixStack.Push(Matrix);
        inverseMatrixStack.Push(InverseMatrix);
    }

    public void PopMatrix()
    {
        GraphicsDevice.EnsureInitialized();

        if (matrixStack.Count == 0) throw new InvalidOperationException("Matrix stack is empty");

        Matrix = matrixStack.Pop();
        InverseMatrix = inverseMatrixStack.Pop();

        UpdateShaderMatrix();
    }

    public void DrawQuad
    (
        DrawMatrix drawMatrix,
        Vector2 position,
        Vector2 size,
        uint packedColor,
        float alpha,
        float borderThickness,
        bool borderHasSingleColor,
        Matrix4x4 borderColor,
        float cornerRadius,
        RectangleF? textureCoord = null,
        Texture? texture = null,
        VertexMode vertexMode = VertexMode.Shape,
        TextureFilterMode filterMode = TextureFilterMode.Linear
    )
    {
        if (texture is { IsUploaded: false })
        {
            if (!texture.UploadQueued) texture.EnqueueUpload();
            return;
        }

        var v0 = position;
        if (texture != GraphicsDevice.CurrentTexture)
        {
            GraphicsDevice.BindTexture(texture, filterMode);
        }

        var v1 = position with { Y = position.Y + size.Y };
        var v2 = position + size;
        var v3 = position with { X = position.X + size.X };

        v0 = Vector2.Transform(v0, drawMatrix.Matrix);
        v1 = Vector2.Transform(v1, drawMatrix.Matrix);
        v2 = Vector2.Transform(v2, drawMatrix.Matrix);
        v3 = Vector2.Transform(v3, drawMatrix.Matrix);

        var tex = textureCoord ?? new Rectangle(0, 0, 1, 1);

        VertexBatch.PushVertex(new Vertex2D(
            position: v0,
            texCoord: new Vector2(tex.Left, tex.Top),
            size: size,
            color: packedColor,
            alpha: alpha,
            radius: cornerRadius,
            localUv: new Vector2(0, 0),
            mode: vertexMode,
            borderThickness: borderThickness,
            hasSingleColor: borderHasSingleColor,
            borderColor: borderColor
        ));

        VertexBatch.PushVertex(new Vertex2D(
            position: v1,
            texCoord: new Vector2(tex.Left, tex.Bottom),
            size: size,
            color: packedColor,
            alpha: alpha,
            radius: cornerRadius,
            localUv: new Vector2(0, 1),
            mode: vertexMode,
            borderThickness: borderThickness,
            hasSingleColor: borderHasSingleColor,
            borderColor: borderColor
        ));

        VertexBatch.PushVertex(new Vertex2D(
            position: v2,
            texCoord: new Vector2(tex.Right, tex.Bottom),
            size: size,
            color: packedColor,
            alpha: alpha,
            radius: cornerRadius,
            localUv: new Vector2(1, 1),
            mode: vertexMode,
            borderThickness: borderThickness,
            hasSingleColor: borderHasSingleColor,
            borderColor: borderColor
        ));

        VertexBatch.PushVertex(new Vertex2D(
            position: v3,
            texCoord: new Vector2(tex.Right, tex.Top),
            size: size,
            color: packedColor,
            alpha: alpha,
            radius: cornerRadius,
            localUv: new Vector2(1, 0),
            mode: vertexMode,
            borderThickness: borderThickness,
            hasSingleColor: borderHasSingleColor,
            borderColor: borderColor
        ));
    }

    public void Translate(float x, float y, float z)
    {
        GraphicsDevice.EnsureInitialized();

        Matrix = Matrix4x4.CreateTranslation(x, y, z) * Matrix;
        InverseMatrix *= Matrix4x4.CreateTranslation(-x, -y, -z);

        UpdateShaderMatrix();
    }

    public void BeginRenderTarget(Framebuffer framebuffer)
    {
        FlushVertexBatch();
        GraphicsDevice.BindFramebuffer(framebuffer);

        projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, framebuffer.Size.X, framebuffer.Size.Y, 0, -1, 1);
        LoadIdentity();
    }

    public void EndRenderTarget()
    {
        FlushVertexBatch();
        GraphicsDevice.UnbindFramebuffer();

        projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, GraphicsDevice.BackBufferWidth, GraphicsDevice.BackBufferHeight, 0, -1, 1);
        LoadIdentity();
    }

    public void ClearCurrentTarget()
    {
        GraphicsDevice.OpenGL.ClearColor(0, 0, 0, 0);
        GraphicsDevice.OpenGL.Clear(ClearBufferMask.ColorBufferBit);
    }

    public void Scale(float x, float y, float z)
    {
        GraphicsDevice.EnsureInitialized();
        Matrix = Matrix4x4.CreateScale(x, y, z) * Matrix;
        InverseMatrix *= Matrix4x4.CreateScale(1 / x, 1 / y, 1 / z);

        UpdateShaderMatrix();
    }

    public void Rotate(float degrees, float x, float y, float z)
    {
        GraphicsDevice.EnsureInitialized();

        var rads = degrees.ToRads();
        var axis = Vector3.Normalize(new Vector3(x, y, z));

        Matrix = Matrix4x4.CreateFromAxisAngle(axis, rads) * Matrix;
        InverseMatrix *= Matrix4x4.CreateFromAxisAngle(axis, -rads);

        UpdateShaderMatrix();
    }

    public void CacheUniformLocations()
    {
        transformMatrixShaderLocation = GraphicsDevice.CurrentShader.GetUniformLocation(shader_uniform_transform_matrix);
    }

    public void LoadIdentity()
    {
        GraphicsDevice.EnsureInitialized();
        Matrix = Matrix4x4.Identity;
        InverseMatrix = Matrix4x4.Identity;
        UpdateShaderMatrix();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ScreenToLocal(Vector2 screenPos)
    {
        return Vector2.Transform(screenPos, InverseMatrix);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ScreenToLocalDirection(Vector2 screenDelta)
    {
        return Vector2.TransformNormal(screenDelta, InverseMatrix);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsPoint(Vector2 screenPos, Vector2 size)
    {
        var localPos = ScreenToLocal(screenPos);
        return localPos.X >= 0 && localPos.X <= size.X &&
               localPos.Y >= 0 && localPos.Y <= size.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsPoint(Vector2 screenPos, Vector2 offset, Vector2 size)
    {
        var localPos = ScreenToLocal(screenPos);
        return localPos.X >= offset.X && localPos.X <= offset.X + size.X &&
               localPos.Y >= offset.Y && localPos.Y <= offset.Y + size.Y;
    }

    public void UpdateShaderMatrix()
    {
        GraphicsDevice.CurrentShader.SetMatrix4(transformMatrixShaderLocation, Matrix * projectionMatrix);
    }

    public void BeginDrawing()
    {
        GraphicsDevice.PushViewport();

        projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(
            0, GraphicsDevice.BackBufferWidth, GraphicsDevice.BackBufferHeight, 0, -1, 1);

        LoadIdentity();
        UpdateShaderMatrix();
    }

    public void EndDrawing()
    {
        GraphicsDevice.EnsureInitialized();
        VertexBatch.Flush();
    }

    public void FlushVertexBatch()
    {
        VertexBatch.Flush();
    }
}
