using Common;
using Common.Logger;
using Common.Util;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Container;

public class ScreenStack : Container2d
{
    public Screen? Current => stack.Count != 0 ? stack.Peek() : null;

    private readonly Stack<Screen> stack = new();

    public ScreenStack()
    {
        RelativeSizeAxes = Axes.Both;
    }

    public CompletableFuture<Screen> Push(Screen newScreen)
    {
        var existingCount = stack.Count(s => s.GetType() == newScreen.GetType());
        if (existingCount > newScreen.MaximumScreenInstances)
        {
            throw new InvalidOperationException($"There can only be maximum {newScreen.MaximumScreenInstances} instances of {newScreen.ObjectName()}");
        }

        return loadScreen(newScreen)
            .Then(screen =>
            {
                var old = Current;
                stack.Push(screen);

                Logger.Debug($"Entering {newScreen.ObjectName()}");

                newScreen.IsTransitioning = true;
                if (old != null) old.IsTransitioning = true;
                AddChild(newScreen);

                var screenTransitionEvent = new ScreenTransitionEvent(Current, newScreen);
                var transitionFuture = old != null ? CompletableFuture.All(newScreen.OnEntering(screenTransitionEvent), old.OnSuspending(screenTransitionEvent)) : newScreen.OnEntering(screenTransitionEvent);
                transitionFuture.Then(_ =>
                {
                    newScreen.IsTransitioning = false;

                    if (old == null) return;

                    old.IsTransitioning = false;
                    old.IsSuspended = true;
                    Logger.Debug($"Suspended {old?.ObjectName()}");
                    Logger.Debug($"New active screen: {newScreen.ObjectName()}");
                    InputManager.InvalidMousePosition();
                });
            });
    }

    public void Pop()
    {
        var old = stack.Pop();

        var screenTransitionEvent = new ScreenTransitionEvent(old, Current);
        
        old.IsTransitioning = true;
        old
            .OnExiting(new ScreenExitEvent(Current))
            .Then(_ =>
            {
                RemoveChild(old);
                InputManager.InvalidMousePosition();
            });

        if (Current == null) return;
        
        Logger.Debug($"Resuming {Current.ObjectName()}");
        Current.IsSuspended = false;
        Current.IsTransitioning = true;
        Current
            .OnResuming(screenTransitionEvent)
            .Then(_ =>
            {
                Current.IsTransitioning = false;
                Logger.Debug($"New active screen: {Current.ObjectName()}");
            });
    }

    private CompletableFuture<Screen> loadScreen(Screen screen)
    {
        var game = DependencyContainer.Get<Game>();
        if (screen.LoadState >= DrawableLoadState.Ready)
        {
            return CompletableFuture.Completed(screen);
        }

        var task = new CompletableFuture<Screen>();

        game.UpdateThread.Schedule(() =>
        {
            Logger.Verbose($"Loading screen {screen.ObjectName()}", Logger.Runtime);
            screen.Load();
            task.Complete(screen);
        });

        return task;
    }
}