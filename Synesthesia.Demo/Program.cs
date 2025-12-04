using Common.Util;
using Synesthesia.Engine;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Host;

namespace Synesthesia.Demo;

public class Program
{
    public static void Main(string[] args)
    {
        EnvUtils.Set("GRAPHICS_RENDERER", RendererType.OpenGL);

        var game = new Game
        {
            Host = new SDL2WindowHost()
        };


        game.Run();
    }
}