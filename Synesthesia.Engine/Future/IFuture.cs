namespace Synesthesia.Engine.Future;

public interface IFuture
{
    bool IsComplete { get; }
    void OnCompleted(Action callback);
}
