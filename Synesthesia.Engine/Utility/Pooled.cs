// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Event;
using Common.Pooling;
using Synesthesia.Engine.Graphics;

namespace Synesthesia.Engine.Utility;

public static class Pooled
{
    public static readonly FastObjectPool<SingleOffEventDispatcher<Drawable>> DRAWABLE_LOAD_DISPATCHER_POOL = new FastObjectPool<SingleOffEventDispatcher<Drawable>>(() => new SingleOffEventDispatcher<Drawable>(), 500);
}
