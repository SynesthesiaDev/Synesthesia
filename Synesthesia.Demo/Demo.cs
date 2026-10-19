using System.Numerics;
using Synesthesia.Engine;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Graphics.Three;

namespace Synesthesia.Demo;

internal static class Demo
{
    [STAThread]
    private static void Main(string[] args)
    {
        var game = new GameBuilder().Build();
        var camera = new Camera
        {
            Position = new Vector3(2, 0, 5)
        };

        game.OnInitialized.Subscribe(_ =>
        {
            game.DrawableScene3D.Children =
            [
                camera,
                new Cube3D
                {
                    Size = new Vector3(1, 1, 1),
                    Color = Color.Red,
                },
                new Cube3D
                {
                    Position = new Vector3(0, 1, 0),
                    Size = new Vector3(1, 1, 1),
                    Color = Color.Green
                }
            ];

            game.RenderThread.GraphicsDevice.Renderer3D.Camera = camera;
        });

        // game.OnInitialized.Subscribe(_ =>
        // {
        //     game.DrawableScene2D.Children =
        //     [
        //         new SideScrollerTest
        //         {
        //             RelativeSizeAxes = Axes.Both
        //         }
        //     ];
        // });

        game.Run();
    }
}
