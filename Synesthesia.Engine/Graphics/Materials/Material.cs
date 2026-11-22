// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Platform.Render;
using Synesthesia.Engine.Resources;

namespace Synesthesia.Engine.Graphics.Materials;

public abstract class Material : INamedResource
{
    private static ulong nextId;

    public ulong StaticBatchId { get; } = Interlocked.Increment(ref nextId);

    public abstract string ResourceName { get; }
    public abstract Shader Shader { get; init; }

    public BlendMode BlendMode { get; init; }
    public DepthTest DepthTest { get; init; }
    public FaceCulling FaceCulling { get; init; }

    public virtual bool IsTransparent { get; init; } = false;

    public virtual TextureFilterMode TextureFilterMode { get; set; } = TextureFilterMode.Trilinear;

    public override string ToString() => Stringify();

    protected abstract string Stringify();

    public bool UniformLocationsCached { get; set; }

    public abstract void Bind(GraphicsDevice graphicsDevice);
    public abstract void CacheUniformLocations();
    protected abstract void OnDispose();

    private int batchKey;
    private bool batchKeyDirty = true;

    protected void InvalidateBatchKey() => batchKeyDirty = true;

    public int GetBatchKey()
    {
        if (!batchKeyDirty) return batchKey;

        batchKey = ComputeBatchKey();
        batchKeyDirty = false;
        return batchKey;
    }

    protected virtual int ComputeBatchKey() => HashCode.Combine(Shader.StaticBatchId, IsTransparent, BlendMode, DepthTest, FaceCulling);

    public void Dispose()
    {
        OnDispose();
        Shader.Dispose();
    }
}
