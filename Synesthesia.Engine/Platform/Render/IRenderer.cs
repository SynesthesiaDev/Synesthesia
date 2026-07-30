// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics;

namespace Synesthesia.Engine.Platform.Render;

public interface IRenderer<T> where T : unmanaged
{
    void BeginDrawing();
    void EndDrawing();
    void FlushVertexBatch();

    void UpdateShaderMatrix();
    void CacheUniformLocations();

    VertexBatch<T> VertexBatch { get; }
}
