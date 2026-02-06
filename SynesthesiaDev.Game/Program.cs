namespace SynesthesiaDev.Game;

internal abstract class Program
{
    private static void Main(string[] args)
    {
        var game = new Synesthesia.Engine.Game();
        
        game.Run();
    }
}
