namespace Common.Util;

public interface IFuture
{
    bool IsComplete { get; }
    void OnCompleted(Action callback);
}