using Common.Util;

namespace Common;

public class CompletableFuture<T> : IFuture
{
    private Action<T>? _then = null;

    private T? _result = default;
    private Exception? _exception = null;

    private List<Action<T>> _successCallbacks = [];
    private List<Action<Exception>> _failCallbacks = [];
    private List<Action> _anyCompletionCallbacks = [];

    public bool IsComplete { get; private set; } = false;

    public CompletableFuture<T> Then(Action<T> then)
    {
        if (IsComplete && _exception == null)
        {
            then.Invoke(_result!);
        }
        else
        {
            _successCallbacks.Add(then);
        }

        return this;
    }
    
    public void Complete(T value)
    {
        if (IsComplete) return;

        _result = value;
        IsComplete = true;
        _successCallbacks.ForEach(a => a.Invoke(value));
        _anyCompletionCallbacks.ForEach(a => a.Invoke());
    }

    public void Fail(Exception ex)
    {
        if (IsComplete) return;
        _exception = ex;
        IsComplete = true;
        _failCallbacks.ForEach(a => a.Invoke(ex));
        _anyCompletionCallbacks.ForEach(a => a.Invoke());
    }


    public void OnCompleted(Action callback)
    {
        if (IsComplete) callback();
        else _anyCompletionCallbacks.Add(callback);
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
            returnedPromise.Complete(Nothing.Instance);
            return returnedPromise;
        }

        var remaining = futures.Length;
        foreach (var future in futures)
        {
            future.OnCompleted(() =>
            {
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    returnedPromise.Complete(Nothing.Instance);
                }
            });
        }

        return returnedPromise;
    }
}