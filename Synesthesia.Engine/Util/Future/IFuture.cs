namespace Synesthesia.Engine.Util.Future;

public interface IFuture
{
    bool IsComplete { get; }
    void OnCompleted(Action callback);
}
