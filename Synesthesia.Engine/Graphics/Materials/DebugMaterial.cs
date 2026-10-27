// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Platform.Render;

namespace Synesthesia.Engine.Graphics.Materials;

public sealed class DebugMaterial : Material
{
    public override string ResourceName { get; } = "Debug Material";
    public override Shader Shader { get; init; }

    protected override string Stringify()
    {
        return "";
    }

    public override void Bind(GraphicsDevice graphicsDevice)
    {
    }

    public override void CacheUniformLocations()
    {
    }

    protected override void OnDispose()
    {
    }
}

