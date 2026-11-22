// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Graphics;

public interface IHasStaticBatchId
{
    /// <summary>
    /// Static ID that is assigned on class creation used in batching because some batched resources may
    /// not be uploaded to the gpu yet and assigned handle by the renderer
    /// </summary>
    ulong StaticBatchId { get; }
}
