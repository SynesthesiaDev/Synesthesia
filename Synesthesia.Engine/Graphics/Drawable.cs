using System.Diagnostics;
using System.Numerics;
using Common.Bindable;
using Common.Event;
using Common.Logger;
using Common.Statistics;
using Raylib_cs;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Timing.Scheduling;

namespace Synesthesia.Engine.Graphics;

public abstract partial class Drawable : IDrawable, IDisposable
{
    protected internal bool IsDisposed { get; private set; }

    internal readonly object LoadLock = new();

    public DrawableLoadState LoadState { get; protected set; }

    public Thread LoadThread { get; private set; } = null!;

    internal readonly BindablePool BindablePool = new();

    public readonly SingleOffEventDispatcher<Drawable> OnLoadComplete;

    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public Vector3 Shear { get; set; } = Vector3.Zero;

    public bool Visible { get; set; } = true;

    public BlendMode BlendMode { get; set; } = BlendMode.Alpha;

    public float Alpha { get; set; } = 1f;

    public bool IsLoaded => LoadState >= DrawableLoadState.Loaded;

    private static readonly StopwatchClock performance_watch = new(true);

    public Lazy<Scheduler> Scheduler;

    public Lazy<Animator> Animator;


    protected Drawable()
    {
        EngineStatistics.DRAWABLES.Increment();

        OnLoadComplete = BindablePool.BorrowSingleOffDispatcher<Drawable>();

        Scheduler = new Lazy<Scheduler>(() => new Scheduler());
        Animator = new Lazy<Animator>(() => new Animator(this.Scheduler.Value));
    }

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

    private void load()
    {
        LoadThread = Thread.CurrentThread;
        var timeBefore = performance_watch.CurrentTime;

        OnLoading();
        LoadAsyncComplete();

        if (!(timeBefore > 1000)) return;

        var loadDuration = performance_watch.CurrentTime - timeBefore;
        var blocking = ThreadSafety.IsUpdateThread;
        var allowedDuration = blocking ? 16.0 : 100.0;

        if (!(loadDuration > allowedDuration)) return;

        if (blocking)
        {
            Logger.Warning($"{ToString()} took {loadDuration:0.00}ms to load (and blocked the update thread)", Logger.Runtime);
        }
        else
        {
            Logger.Verbose($"{ToString()} took {loadDuration:0.00}ms to load", Logger.Runtime);
        }
    }

    protected virtual void OnLoading()
    {
    }

    private bool loadComplete()
    {
        if (LoadState < DrawableLoadState.Ready) return false;

        LoadState = DrawableLoadState.Loaded;

        // OnUpdate();
        //TODO Update layout

        LoadComplete();

        OnLoadComplete.Dispatch(this);
        return true;
    }

    protected internal abstract void OnDraw();

    protected internal virtual void OnUpdate(FrameInfo frameInfo)
    {
    }

    protected virtual void LoadAsyncComplete()
    {
    }

    protected virtual void LoadComplete()
    {
    }

    public void Dispose()
    {
        lock (LoadLock) Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing)
    {
        if (IsDisposed) return;

        BindablePool.Dispose();
        IsDisposed = true;
        Scheduler.Value.Dispose();

        EngineStatistics.DRAWABLES.Decrement();
    }

    protected Color ApplyAlpha(Color color)
    {
        return color with { A = (byte)(Alpha * 255) };
    }
}
