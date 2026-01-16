using Common.Logger;
using SDL2;
using Synesthesia.Engine.Input;

namespace Synesthesia.Engine.Threading.Runners;

public class InputThreadRunner : IThreadRunner
{
    private Game _game = null!;

    protected override void OnThreadInit(Game game)
    {
        _game = game;
        // SDL.SDL_SetHint(SDL.SDL_HINT_MOUSE_RELATIVE_SCALING, "1");
    }

    public override void OnLoadComplete(Game game)
    {
    }

    protected override void OnLoop()
    {
        try
        {
            InputManager.ProcessQueue(_game);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, Logger.INPUT);
        }
        
    }
}