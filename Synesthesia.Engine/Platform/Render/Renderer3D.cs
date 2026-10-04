// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics;

namespace Synesthesia.Engine.Platform.Render;

public class Renderer3D(GraphicsDevice graphicsDevice) : IRenderer<Vertex3D>
{
    public VertexBatch<Vertex3D> VertexBatch { get; } = new VertexBatch<Vertex3D>(graphicsDevice.OpenGL, is2D: false);

    public void BeginDrawing()
    {

    }

    public void EndDrawing()
    {
    }

    public void FlushVertexBatch()
    {
    }

    public void UpdateShaderMatrix()
    {
    }

    public void CacheUniformLocations()
    {
    }

}
