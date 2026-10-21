// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Platform.Render;

namespace Synesthesia.Engine.Resources.Stores;

public interface IResolvesGraphicsDeferred
{
    void ResolveGraphicsDeferred(GraphicsDevice graphicsDevice);
}
