using System.Diagnostics;
using System.Numerics;
using Common.Bindable;
using Common.Event;
using Common.Logger;
using Common.Statistics;
using Raylib_cs;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Graphics;

public abstract partial class Drawable : IDrawable, IDisposable
{
    protected internal bool IsDisposed { get; private set; }

    internal readonly object LoadLock = new object();

    public DrawableLoadState LoadState { get; protected set; }

    public Thread LoadThread { get; private set; } = null!;

    internal readonly BindablePool BindablePool = new();

    public readonly EventDispatcher<Drawable> OnLoadComplete;

    public readonly EventDispatcher<Drawable> OnInvalidated;

    public readonly EventDispatcher<Drawable> OnDisposed;

    public Vector3 Rotation { get; set; } = new();

    public Vector3 Shear { get; set; } = new();

    public bool Visible { get; set; } = true;

    public BlendMode BlendMode { get; set; } = BlendMode.Alpha;

    public float Alpha { get; set; } = 1f;

    public bool IsLoaded => LoadState >= DrawableLoadState.Loaded;

    private static readonly StopwatchClock performance_watch = new(true);

    protected Drawable()
    {
        EngineStatistics.Drawables.Increment();

        OnLoadComplete = BindablePool.BorrowDispatcher<Drawable>();
        OnDisposed = BindablePool.BorrowDispatcher<Drawable>();
        OnInvalidated = BindablePool.BorrowDispatcher<Drawable>();
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
        //Perfm loading
        // inject deps
        var timeBefore = performance_watch.CurrentTime;

        OnLoading();
        LoadAsyncComplete();

        if (!(timeBefore > 1000)) return;

        var loadDuration = performance_watch.CurrentTime - timeBefore;
        var blocking = ThreadSafety.IsInputThread;
        var allowedDuration = blocking ? 16.0 : 100.0;

        if (!(loadDuration > allowedDuration)) return;

        if (blocking)
        {
            Logger.Warning($"{ToString()} took {loadDuration:0.00}ms to load (and blocked the update thread)", Logger.RUNTIME);
        }
        else
        {
            Logger.Verbose($"{ToString()} took {loadDuration:0.00}ms to load", Logger.RUNTIME);
        }
    }

    protected virtual void OnLoading()
    {
    }

    private bool loadComplete()
    {
        if (LoadState < DrawableLoadState.Ready) return false;

        LoadState = DrawableLoadState.Loaded;

        LoadComplete();

        OnLoadComplete.Dispatch(this);
        // BindablePool.UnregisterDispatcher(OnLoadComplete);
        return true;
    }

    protected internal abstract void OnDraw();

    protected internal virtual void OnUpdate()
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

        OnDisposed.Dispatch(this);
        BindablePool.Dispose();

        IsDisposed = true;

        EngineStatistics.Drawables.Decrement();
    }

    protected Color applyAlpha(Color color)
    {
        return color with { A = (byte)(Alpha * 255) };
    }
}