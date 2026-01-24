using Common.Util;

namespace Common;

public class CompletableFuture<T> : IFuture
{
    private Action<T>? then = null;

    private T? result = default;
    private Exception? exception = null;

    private List<Action<T>> successCallbacks = [];
    private List<Action<Exception>> failCallbacks = [];
    private List<Action> anyCompletionCallbacks = [];

    public bool IsComplete { get; private set; } = false;

    public CompletableFuture<T> Then(Action<T> then)
    {
        if (IsComplete && exception == null)
        {
            then.Invoke(result!);
        }
        else
        {
            successCallbacks.Add(then);
        }

        return this;
    }
    
    public void Complete(T value)
    {
        if (IsComplete) return;

        result = value;
        IsComplete = true;
        successCallbacks.ForEach(a => a.Invoke(value));
        anyCompletionCallbacks.ForEach(a => a.Invoke());
    }

    public void Fail(Exception ex)
    {
        if (IsComplete) return;
        exception = ex;
        IsComplete = true;
        failCallbacks.ForEach(a => a.Invoke(ex));
        anyCompletionCallbacks.ForEach(a => a.Invoke());
    }


    public void OnCompleted(Action callback)
    {
        if (IsComplete) callback();
        else anyCompletionCallbacks.Add(callback);
    }
}

public static class CompletableFuture
{
    public static CompletableFuture<T> Completed<T>(T value)
    {
        var future = new CompletableFuture<T>();
        future.Complete(value);
        return future;
    }

    public static CompletableFuture<Nothing> All(params IFuture[] futures)
    {
        var returnedPromise = new CompletableFuture<Nothing>();
        if (futures.Length == 0)
        {
            returnedPromise.Complete(Nothing.INSTANCE);
            return returnedPromise;
        }

        var remaining = futures.Length;
        foreach (var future in futures)
        {
            future.OnCompleted(() =>
            {
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    returnedPromise.Complete(Nothing.INSTANCE);
                }
            });
        }

        return returnedPromise;
    }
}