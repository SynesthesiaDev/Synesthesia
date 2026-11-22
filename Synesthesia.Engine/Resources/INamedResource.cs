// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics;

namespace Synesthesia.Engine.Resources;

public interface INamedResource : IResource, IHasStaticBatchId
{
    string ResourceName { get; }
}
