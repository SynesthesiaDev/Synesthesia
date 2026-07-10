// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Globalization;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Events;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util.Pooling;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Graphics;

public abstract class Drawable : IDisposable
{
    protected internal bool IsDisposed { get; private set; }

    internal readonly object LoadLock = new();

    public readonly SingleOffEventDispatcher<Drawable> OnLoadComplete = Pooled.DRAWABLE_LOAD_DISPATCHER_POOL.Rent();

    private static readonly StopwatchClock performance_watch = new(true);

    public DrawableLoadState LoadState { get; protected set; }

    public Thread LoadThread { get; private set; } = null!;

    public float Alpha { get; set; } = 1f;

    public BlendMode BlendMode { get; set; } = BlendMode.Alpha;

    public readonly DrawMatrix DrawMatrix = Pooled.DRAW_MATRIX_POOL.Rent();

    protected readonly Lazy<Animator> Animator = new(() => new Animator());

    protected internal abstract void OnDraw();

    protected internal virtual void OnUpdate(FrameInfo frameInfo)
    {
    }

    #region Loading

    public enum DrawableLoadState
    {
        NotLoaded,
        Loading,
        Ready,
        Loaded
    }

    internal void Load()
    {
        lock (LoadLock)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (LoadState != DrawableLoadState.NotLoaded) return;

            Trace.Assert(LoadState == DrawableLoadState.NotLoaded);
            LoadState = DrawableLoadState.Loading;

            load();

            LoadState = DrawableLoadState.Ready;

            loadComplete();
        }
    }

    protected virtual void InternalLoadComplete()
    {
    }

    private void load()
    {
        LoadThread = Thread.CurrentThread;
        var timeBefore = performance_watch.CurrentTime;

        Reflection.ResolveDependencies(this);
        EngineStatistics.Increment(EngineStatistics.Type.Drawables);

        OnLoading();

        if (this is Drawable2D drawable)
        {
            drawable.Invalidate(Invalidation.All);
            drawable.Parent?.Invalidate(Invalidation.All);
        }

        if (!(timeBefore > 1000)) return;

        var loadDuration = performance_watch.CurrentTime - timeBefore;
        var blocking = ThreadSafety.IsUpdateThread;
        var allowedDuration = blocking ? 16.0 : 100.0;

        if (!(loadDuration > allowedDuration)) return;

        if (blocking)
        {
            Logger.Warning(string.Create(CultureInfo.InvariantCulture, $"{ToString()} took {loadDuration:0.00}ms to load (and blocked the update thread)"), Logger.Runtime);
        }
        else
        {
            Logger.Verbose(string.Create(CultureInfo.InvariantCulture, $"{ToString()} took {loadDuration:0.00}ms to load"), Logger.Runtime);
        }
    }

    protected virtual void OnLoading()
    {
    }

    private bool loadComplete()
    {
        if (LoadState < DrawableLoadState.Ready) return false;

        LoadState = DrawableLoadState.Loaded;

        InternalLoadComplete();

        if (this is Drawable2D drawable2D)
        {
            drawable2D.Invalidate(Invalidation.All);
        }

        LoadComplete();

        OnLoadComplete.Dispatch(this);
        return true;
    }

    protected virtual void LoadComplete()
    {
    }

    #endregion

    public void Dispose()
    {
        lock (LoadLock) Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing)
    {
        if (IsDisposed) return;

        Pooled.DRAWABLE_LOAD_DISPATCHER_POOL.Return(OnLoadComplete);
        Pooled.DRAW_MATRIX_POOL.Return(DrawMatrix);
        IsDisposed = true;
        // Scheduler.Value.Dispose(); //TODO

#if DEBUG
        Reflection.CheckForDisposing(this);
#endif
        EngineStatistics.Decrement(EngineStatistics.Type.Drawables);
    }
}
