// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Events;
using Synesthesia.Engine.Graphics;

namespace Synesthesia.Engine.Util.Pooling;

public static class Pooled
{
    public static readonly FastObjectPool<SingleOffEventDispatcher<Drawable>> DRAWABLE_LOAD_DISPATCHER_POOL = new FastObjectPool<SingleOffEventDispatcher<Drawable>>(() => new SingleOffEventDispatcher<Drawable>(), 500);
    public static readonly FastObjectPool<EventDispatcher<string>> STRING_DISPATCHER_POOL = new FastObjectPool<EventDispatcher<string>>(() => new EventDispatcher<string>(), 1);
}
