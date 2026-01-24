
namespace Synesthesia.Engine.Utility;

public abstract class DeferredActionQueue
{
    private bool immediate = false;
    
    private readonly List<Action> queuedActions = [];

    public void FlushAndSwitchToImmediate()
    {
        immediate = true;
        queuedActions.ForEach(p => p.Invoke());
    } 
    
    public void Enqueue(Action action)
    {
        if (immediate)
        {
            action.Invoke();
        }
        else
        {
            queuedActions.Add(action);
        }
    }
}