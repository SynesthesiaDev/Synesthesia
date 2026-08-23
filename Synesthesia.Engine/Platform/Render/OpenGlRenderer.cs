// // Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// // See the LICENCE file in the repository root for full licence text.
//
// using System.Collections.Concurrent;
// using System.Drawing;
// using System.Numerics;
// using System.Runtime.CompilerServices;
// using SDL3;
// using Silk.NET.OpenGL;
// using Synesthesia.Engine.Graphics;
// using Synesthesia.Engine.Graphics.Shaders;
// using Synesthesia.Engine.Logging;
// using Synesthesia.Engine.Threading;
// using Synesthesia.Engine.Util;
// using Synesthesia.Engine.Util.Statistics;
// using Synesthesia.Utils.Extensions;
// using Shader = Synesthesia.Engine.Graphics.Shader;
// using Texture = Synesthesia.Engine.Graphics.Textures.Texture;
//
// namespace Synesthesia.Engine.Platform.Render;
//
// public sealed class OpenGlRenderer : IDisposable
// {
//     private const ClearFlags default_clear_flags = ClearFlags.ColorBuffer | ClearFlags.DepthBuffer | ClearFlags.StencilBuffer;
//
//     private const string shader_uniform_texture = "u_texture";
//     private const string shader_uniform_use_texture = "u_useTexture";
//     private const string shader_uniform_transform_matrix = "u_transform";
//     public required OpenGLSurface Surface { get; init; }
//
//     private bool openGlInitialized;
//
//     public int BackBufferWidth { get; private set; }
//
//     public int BackBufferHeight { get; private set; }
//
//     public ClearFlags ClearFlags = default_clear_flags;
//
//     public Shader DefaultShader = null!;
//     public Shader StencilShader = null!;
//
//     public VertexBatch<Vertex2D> VertexBatch2D = null!;
//
//     public GL OpenGL
//     {
//         get
//         {
//             EnsureInitialized();
//             return field;
//         }
//
//         private set;
//     } = null!;
//
//     public bool CanDraw => BackBufferWidth > 0 && BackBufferHeight > 0;
//
//     private readonly Stack<Matrix4x4> matrixStack = new Stack<Matrix4x4>();
//
//     private readonly Stack<Matrix4x4> inverseMatrixStack = new();
//
//     public int StackDepth => matrixStack.Count;
//
//     public Shader CurrentShader { get; private set; } = null!;
//
//     public Texture? CurrentTexture { get; private set; }
//
//     public Matrix4x4 Matrix { get; private set; } = Matrix4x4.Identity;
//
//     public Matrix4x4 InverseMatrix { get; private set; } = Matrix4x4.Identity;
//
//     private Matrix4x4 projectionMatrix;
//
//     public Matrix4x4 View3D { get; private set; } = Matrix4x4.Identity;
//     public Matrix4x4 Projection3D { get; private set; } = Matrix4x4.Identity;
//
//     public static readonly ConcurrentQueue<Texture> TEXTURE_UPLOAD_QUEUE = new ConcurrentQueue<Texture>();
//
//     // Cache so we don't query with string every frame. That's expensive on gc allocations!!
//     private int textureShaderLocation;
//     private int useTextureShaderLocation;
//     private int transformMatrixShaderLocation;
//     private int stencilDepthStack;
//
//     public void Initialize()
//     {
//         if (openGlInitialized) throw new InvalidOperationException("OpenGL is already initialized");
//
//         var gl = GL.GetApi(name =>
//         {
//             var ptr = OpenGLSurface.GetProcAddress(name);
//             return ptr;
//         });
//
//         OpenGL = gl ?? throw new InvalidOperationException("Silk.NET could not bind to OpenGL");
//
//         BackBufferHeight = Surface.BackBufferHeight;
//         BackBufferWidth = Surface.BackBufferWidth;
//
//         openGlInitialized = true;
//         Resize(BackBufferWidth, BackBufferHeight);
//
//         var version = OpenGL.GetStringS(GLEnum.Version);
//         var shadingLanguageVersion = OpenGL.GetStringS(GLEnum.ShadingLanguageVersion);
//         var vendor = OpenGL.GetStringS(GLEnum.Vendor);
//         var renderer = OpenGL.GetStringS(GLEnum.Renderer);
//
//         VertexBatch2D = new VertexBatch<Vertex2D>(OpenGL);
//
//         Logger.EmptyLine();
//         Logger.Debug("OpenGL Initialized", Logger.Platform);
//         Logger.Debug($"- Version:   {version}", Logger.Platform);
//         Logger.Debug($"- Vendor:    {vendor}", Logger.Platform);
//         Logger.Debug($"- Renderer   {renderer}", Logger.Platform);
//         Logger.Debug($"- GLSL:      {shadingLanguageVersion}", Logger.Platform);
//     }
//
//     public void DrawQuad
//     (
//         DrawMatrix drawMatrix,
//         Vector2 position,
//         Vector2 size,
//         uint packedColor,
//         float alpha,
//         float borderThickness,
//         bool borderHasSingleColor,
//         Matrix4x4 borderColor,
//         float cornerRadius,
//         RectangleF? textureCoord = null,
//         Texture? texture = null,
//         VertexMode vertexMode = VertexMode.Shape
//     )
//     {
//         if (texture is { IsUploaded: false }) return;
//
//         if (texture != CurrentTexture)
//         {
//             BindTexture(texture);
//         }
//
//         var v0 = position;
//         var v1 = position with { Y = position.Y + size.Y };
//         var v2 = position + size;
//         var v3 = position with { X = position.X + size.X };
//
//         v0 = Vector2.Transform(v0, drawMatrix.Matrix);
//         v1 = Vector2.Transform(v1, drawMatrix.Matrix);
//         v2 = Vector2.Transform(v2, drawMatrix.Matrix);
//         v3 = Vector2.Transform(v3, drawMatrix.Matrix);
//
//         var tex = textureCoord ?? new Rectangle(0, 0, 1, 1);
//
//         VertexBatch2D.PushVertex(new Vertex2D(
//             position: v0,
//             texCoord: new Vector2(tex.Left, tex.Top),
//             size: size,
//             color: packedColor,
//             alpha: alpha,
//             radius: cornerRadius,
//             localUv: new Vector2(0, 0),
//             mode: vertexMode,
//             borderThickness: borderThickness,
//             hasSingleColor: borderHasSingleColor,
//             borderColor: borderColor
//         ));
//
//         VertexBatch2D.PushVertex(new Vertex2D(
//             position: v1,
//             texCoord: new Vector2(tex.Left, tex.Bottom),
//             size: size,
//             color: packedColor,
//             alpha: alpha,
//             radius: cornerRadius,
//             localUv: new Vector2(0, 1),
//             mode: vertexMode,
//             borderThickness: borderThickness,
//             hasSingleColor: borderHasSingleColor,
//             borderColor: borderColor
//         ));
//
//         VertexBatch2D.PushVertex(new Vertex2D(
//             position: v2,
//             texCoord: new Vector2(tex.Right, tex.Bottom),
//             size: size,
//             color: packedColor,
//             alpha: alpha,
//             radius: cornerRadius,
//             localUv: new Vector2(1, 1),
//             mode: vertexMode,
//             borderThickness: borderThickness,
//             hasSingleColor: borderHasSingleColor,
//             borderColor: borderColor
//         ));
//
//         VertexBatch2D.PushVertex(new Vertex2D(
//             position: v3,
//             texCoord: new Vector2(tex.Right, tex.Top),
//             size: size,
//             color: packedColor,
//             alpha: alpha,
//             radius: cornerRadius,
//             localUv: new Vector2(1, 0),
//             mode: vertexMode,
//             borderThickness: borderThickness,
//             hasSingleColor: borderHasSingleColor,
//             borderColor: borderColor
//         ));
//     }
//
//     public void CompileDefaultShaders()
//     {
//         DefaultShader = new Shader(OpenGL, ShaderSources.DefaultVertex, ShaderSources.DefaultFragment);
//         StencilShader = new Shader(OpenGL, ShaderSources.DefaultVertex, ShaderSources.StencilFragment);
//         BindShader(DefaultShader);
//     }
//
//     public void BindShader(Shader shader)
//     {
//         ThreadSafety.AssertRunningOnRenderThread();
//
//         if (CurrentShader == shader) return;
//         // Flush any pending vertices BEFORE swapping shaders
//         if (openGlInitialized)
//             VertexBatch2D.Flush();
//
//         CurrentShader = shader;
//         shader.Use();
//         updateShaderMatrix();
//         cacheShaderUniformLocations();
//
//         if (CurrentTexture != null)
//         {
//             CurrentShader.SetInt(textureShaderLocation, 0);
//             CurrentShader.SetInt(useTextureShaderLocation, 1);
//         }
//         else
//         {
//             CurrentShader.SetInt(useTextureShaderLocation, 0);
//         }
//     }
//
//     public void UnbindShader()
//     {
//         ThreadSafety.AssertRunningOnRenderThread();
//         BindShader(DefaultShader);
//         updateShaderMatrix();
//     }
//
//     private void cacheShaderUniformLocations()
//     {
//         textureShaderLocation = CurrentShader.GetUniformLocation(shader_uniform_texture);
//         useTextureShaderLocation = CurrentShader.GetUniformLocation(shader_uniform_use_texture);
//         transformMatrixShaderLocation = CurrentShader.GetUniformLocation(shader_uniform_transform_matrix);
//     }
//
//     public void BindTexture(Texture? texture)
//     {
//         ThreadSafety.AssertRunningOnRenderThread();
//         if (CurrentTexture == texture) return;
//
//         VertexBatch2D.Flush();
//         CurrentTexture = texture;
//         if (texture != null && texture.Bind(OpenGL))
//         {
//             CurrentShader.SetInt(textureShaderLocation, 0);
//             CurrentShader.SetInt(useTextureShaderLocation, 1);
//         }
//         else
//         {
//             OpenGL.BindTexture(TextureTarget.Texture2D, 0);
//             CurrentShader.SetInt(useTextureShaderLocation, 0);
//         }
//
//         //
//         // if (texture is { IsUploaded: true })
//         // {
//         //     texture.Bind(OpenGL);
//         //     CurrentShader.SetInt(textureShaderLocation, 0);
//         //     CurrentShader.SetInt(useTextureShaderLocation, 1);
//         // }
//         // else
//         // {
//         //     OpenGL.BindTexture(TextureTarget.Texture2D, 0);
//         //     CurrentShader.SetInt(useTextureShaderLocation, 0);
//         // }
//     }
//
//     public void Resize(int width, int height)
//     {
//         EnsureInitialized();
//         pushViewport2D();
//     }
//
//     private void pushViewport2D()
//     {
//         SDL.GetWindowSizeInPixels(Surface.WindowHandle, out int w, out int h);
//         BackBufferWidth = w;
//         BackBufferHeight = h;
//
//         projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, w, h, 0, -1, 1);
//
//         OpenGL.Viewport(0, 0, (uint)w, (uint)h);
//     }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public void EnsureInitialized()
//     {
//         if (!openGlInitialized) throw new InvalidOperationException("OpenGL is not initialized yet");
//     }
//
//     public void BeginDrawing3D(Matrix4x4 view, Matrix4x4 projection)
//     {
//         EnsureInitialized();
//
//         OpenGL.Enable(EnableCap.DepthTest);
//         OpenGL.DepthFunc(DepthFunction.Less);
//         OpenGL.Enable(EnableCap.CullFace);
//         OpenGL.CullFace(TriangleFace.Back);
//         OpenGL.FrontFace(FrontFaceDirection.Ccw);
//
//         View3D = view;
//         Projection3D = projection;
//     }
//
//     public void EndDrawing3D()
//     {
//         OpenGL.Disable(EnableCap.DepthTest);
//         OpenGL.Disable(EnableCap.CullFace);
//     }
//
//     public void BeginDrawing2D()
//     {
//         stencilDepthStack = 0;
//         pushViewport2D();
//
//         LoadIdentity();
//         updateShaderMatrix();
//         DrawStatistics.Reset();
//     }
//
//     public void BeginDrawing()
//     {
//         EnsureInitialized();
//
//         ClearBufferMask mask = ClearBufferMask.None;
//
//         if (ClearFlags.HasFlagFast(ClearFlags.ColorBuffer))
//             mask |= ClearBufferMask.ColorBufferBit;
//         if (ClearFlags.HasFlagFast(ClearFlags.DepthBuffer))
//             mask |= ClearBufferMask.DepthBufferBit;
//         if (ClearFlags.HasFlagFast(ClearFlags.StencilBuffer))
//             mask |= ClearBufferMask.StencilBufferBit;
//
//         if (mask != ClearBufferMask.None) OpenGL.Clear(mask);
//
//         DrawStatistics.Set(DrawStatistics.Type.TextureUploadQueue, TEXTURE_UPLOAD_QUEUE.Count);
//
//         while (!TEXTURE_UPLOAD_QUEUE.IsEmpty)
//         {
//             TEXTURE_UPLOAD_QUEUE.TryDequeue(out var texture);
//             texture?.Upload(OpenGL);
//         }
//     }
//
//     public void EndDrawing()
//     {
//         EnsureInitialized();
//
//         Surface.SwapBuffers();
//         BindTexture(null);
//         ClearFlags = default_clear_flags;
//     }
//
//     public void EndDrawing2D()
//     {
//         EnsureInitialized();
//         VertexBatch2D.Flush();
//     }
//
//     public void PushMatrix()
//     {
//         EnsureInitialized();
//         matrixStack.Push(Matrix);
//         inverseMatrixStack.Push(InverseMatrix);
//     }
//
//     public void PopMatrix()
//     {
//         EnsureInitialized();
//
//         if (matrixStack.Count == 0) throw new InvalidOperationException("Matrix stack is empty");
//
//         Matrix = matrixStack.Pop();
//         InverseMatrix = inverseMatrixStack.Pop();
//
//         updateShaderMatrix();
//     }
//
//     public void Translate(float x, float y, float z)
//     {
//         EnsureInitialized();
//
//         Matrix = Matrix4x4.CreateTranslation(x, y, z) * Matrix;
//         InverseMatrix *= Matrix4x4.CreateTranslation(-x, -y, -z);
//
//         updateShaderMatrix();
//     }
//
//     public void Scale(float x, float y, float z)
//     {
//         EnsureInitialized();
//         Matrix = Matrix4x4.CreateScale(x, y, z) * Matrix;
//         InverseMatrix *= Matrix4x4.CreateScale(1 / x, 1 / y, 1 / z);
//
//         updateShaderMatrix();
//     }
//
//     public void Rotate(float degrees, float x, float y, float z)
//     {
//         EnsureInitialized();
//
//         var rads = degrees.ToRads();
//         var axis = Vector3.Normalize(new Vector3(x, y, z));
//
//         Matrix = Matrix4x4.CreateFromAxisAngle(axis, rads) * Matrix;
//         InverseMatrix *= Matrix4x4.CreateFromAxisAngle(axis, -rads);
//
//         updateShaderMatrix();
//     }
//
//     public void LoadIdentity()
//     {
//         EnsureInitialized();
//         Matrix = Matrix4x4.Identity;
//         InverseMatrix = Matrix4x4.Identity;
//         updateShaderMatrix();
//     }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public Vector2 ScreenToLocal(Vector2 screenPos)
//     {
//         return Vector2.Transform(screenPos, InverseMatrix);
//     }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public Vector2 ScreenToLocalDirection(Vector2 screenDelta)
//     {
//         return Vector2.TransformNormal(screenDelta, InverseMatrix);
//     }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public bool ContainsPoint(Vector2 screenPos, Vector2 size)
//     {
//         var localPos = ScreenToLocal(screenPos);
//         return localPos.X >= 0 && localPos.X <= size.X &&
//                localPos.Y >= 0 && localPos.Y <= size.Y;
//     }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public bool ContainsPoint(Vector2 screenPos, Vector2 offset, Vector2 size)
//     {
//         var localPos = ScreenToLocal(screenPos);
//         return localPos.X >= offset.X && localPos.X <= offset.X + size.X &&
//                localPos.Y >= offset.Y && localPos.Y <= offset.Y + size.Y;
//     }
//
//     private void updateShaderMatrix()
//     {
//         CurrentShader.SetMatrix4(transformMatrixShaderLocation, Matrix * projectionMatrix);
//     }
//
//     public void RotateAround(Vector2 pivot, float degrees)
//     {
//         EnsureInitialized();
//
//         Translate(-pivot.X, -pivot.Y, 0);
//         Rotate(degrees, 0, 0, 1);
//         Translate(pivot.X, pivot.Y, 0);
//     }
//
//     public void BeginStencil()
//     {
//         if (stencilDepthStack == 0)
//         {
//             OpenGL.Enable(GLEnum.StencilTest);
//         }
//
//         stencilDepthStack++;
//     }
//
//     public void BeginStencilMask()
//     {
//         OpenGL.Enable(EnableCap.Multisample);
//         OpenGL.ColorMask(false, false, false, false);
//
//         OpenGL.StencilFunc(StencilFunction.Equal, stencilDepthStack - 1, 0xFF);
//         OpenGL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
//
//         BindShader(StencilShader);
//     }
//
//     public void EndStencilMask()
//     {
//         OpenGL.ColorMask(true, true, true, true);
//
//         OpenGL.StencilFunc(StencilFunction.Equal, stencilDepthStack, 0xFF);
//         OpenGL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
//
//         UnbindShader();
//     }
//
//     public void EndStencil()
//     {
//         stencilDepthStack--;
//
//         if (stencilDepthStack == 0)
//         {
//             OpenGL.Disable(EnableCap.StencilTest);
//         }
//         else
//         {
//             OpenGL.StencilFunc(StencilFunction.Equal, stencilDepthStack, 0xFF);
//             OpenGL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
//         }
//
//         OpenGL.ColorMask(true, true, true, true);
//         UnbindShader();
//     }
//
//     public void BeginStencilRestore()
//     {
//         OpenGL.Enable(EnableCap.Multisample);
//         OpenGL.ColorMask(false, false, false, false);
//
//         OpenGL.StencilFunc(StencilFunction.Equal, stencilDepthStack, 0xFF);
//         OpenGL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Decr);
//
//         BindShader(StencilShader);
//     }
//
//     public void Dispose()
//     {
//         openGlInitialized = false;
//         OpenGL.Dispose();
//     }
// }
