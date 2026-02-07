using System.Numerics;
using Synesthesia.Engine.Graphics.Three.Shapes;

namespace SynesthesiaDev.Game;

internal abstract class Program
{
    private static Player player = null!;

    private static void Main(string[] args)
    {
        var game = new Synesthesia.Engine.Game();

        game.DeferredActionQueue.Enqueue(() =>
        {
            game.RootComposite3d.Children =
            [
                player = new Player(),

                new Cube
                {
                    Position = Vector3.Zero,
                    Size = new Vector3(2, 2, 2)
                },

                new DrawableGrid3d
                {
                    Spacing = 1f,
                    Slices = 20,
                },
            ];

            game.RootComposite3d.ActiveCamera3d = player.Camera;
            game.ConsumesCursor.Value = true;
        });


        game.Run();
    }
}
