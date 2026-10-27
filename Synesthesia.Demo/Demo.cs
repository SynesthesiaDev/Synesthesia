using System.Numerics;
using Synesthesia.Engine;
using Synesthesia.Engine.Graphics.Materials;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Utils.Randomness;

namespace Synesthesia.Demo;

internal static class Demo
{
    [STAThread]
    private static void Main(string[] args)
    {
        var game = new GameBuilder().Build();
        // var camera = new Camera
        // {
        //     Position = new Vector3(2, 0, 60)
        // };

        game.OnInitialized.Subscribe(_ =>
        {
            // var catto = DefaultMaterial.From(game.TextureResourceStore.Get("Synesthesia.Resources.Textures.cat.png"));
            var prototype = new PrototypeMaterial();
            game.DrawableScene3D.Children =
            [
                new EditorController
                {
                    Position = new Vector3(2, 0, 60)
                }
            ];

            for (int i = 0; i < 100; i++)
            {
                game.DrawableScene3D.AddChild(new Cube3D
                {
                    Position = new Vector3(Rng.RandomFloat(-30f, 30f), Rng.RandomFloat(-30f, 30f), Rng.RandomFloat(-30f, 30f)),
                    Rotation = new Vector3(Rng.RandomFloat(-20f, 20f), Rng.RandomFloat(-10f, 10f), Rng.RandomFloat(-10f, 10f)),
                    Material = prototype
                });
            }

            // game.RenderThread.GraphicsDevice.Renderer3D.Camera = camera;
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
