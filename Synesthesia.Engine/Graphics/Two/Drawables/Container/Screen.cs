using Common;
using Common.Util;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Container;

public abstract class Screen : Container2d
{
    public bool IsSuspended { get; set; } = false;
    
    public bool IsTransitioning { get; set; } = false;

    protected internal override bool AcceptsInputs() => !IsSuspended && !IsTransitioning;

    public int? MaximumScreenInstances { get; set; } = null;

    protected Screen()
    {
        RelativeSizeAxes = Axes.Both;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
    }

    protected internal override void OnUpdate()
    {
        if (IsSuspended) return;
        base.OnUpdate();
    }

    public virtual CompletableFuture<Nothing> OnEntering(ScreenTransitionEvent e) => CompletableFuture.Completed(Nothing.Instance);

    public virtual CompletableFuture<Nothing> OnExiting(ScreenExitEvent e) => CompletableFuture.Completed(Nothing.Instance);

    public virtual CompletableFuture<Nothing> OnResuming(ScreenTransitionEvent e) => CompletableFuture.Completed(Nothing.Instance);

    public virtual CompletableFuture<Nothing> OnSuspending(ScreenTransitionEvent e) => CompletableFuture.Completed(Nothing.Instance);
}