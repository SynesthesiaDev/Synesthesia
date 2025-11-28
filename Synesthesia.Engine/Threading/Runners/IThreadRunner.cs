using System.Collections.Concurrent;
using Common.Logger;

namespace Synesthesia.Engine.Threading.Runners;

public abstract class IThreadRunner : IDisposable
{
    public Thread Thread { get; private set; } = null!;

    private ConcurrentQueue<Action> _workQueue = new();

    private Game _game = null!;

    private bool _isRunning;

    protected TimeSpan targetUpdateTime = TimeSpan.FromSeconds(1.0 / 60);

    public void SetTargetUpdateTime(long newInputRate)
    {
        targetUpdateTime = TimeSpan.FromSeconds(1.0 / newInputRate);
    }

    protected abstract void OnLoop();

    protected abstract void OnInit(Game game);

    public void Schedule(Action func)
    {
        _workQueue.Enqueue(func);
    }

    private void ExecuteScheduledActions()
    {
        while (_workQueue.TryDequeue(out var workItem))
        {
            workItem.Invoke();
        }
    }

    public void Start(Thread thread, Game game)
    {
        _game = game;
        Thread = thread;
        Thread.Start();
        _isRunning = true;
    }

    public void InternalLoop()
    {
        OnInit(_game);

        while (_isRunning)
        {
            var now = DateTime.UtcNow;
            try
            {
                OnLoop();
                ExecuteScheduledActions();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, Logger.RUNTIME);
            }

            var elapsed = DateTime.UtcNow - now;
            var sleepTime = targetUpdateTime - elapsed;
            if (sleepTime > TimeSpan.Zero)
            {
                Thread.Sleep(sleepTime);
            }
        }
    }

    public void Dispose()
    {
        _isRunning = false;
        _workQueue.Clear();
    }
}