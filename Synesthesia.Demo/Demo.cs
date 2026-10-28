using System.Numerics;
using Synesthesia.Engine;
using Synesthesia.Engine.Graphics.Materials;
using Synesthesia.Engine.Graphics.Three;

namespace Synesthesia.Demo;

internal static class Demo
{
    [STAThread]
    private static void Main(string[] args)
    {
        var game = new GameBuilder().Build();

        game.OnInitialized.Subscribe(_ =>
        {
            var prototype = PrototypeMaterial.From(game.TextureResourceStore.Get("Synesthesia.Resources.Textures.Prototype.gray.png"));
            game.DrawableScene3D.Children =
            [
                new EditorController
                {
                    Position = new Vector3(2, 0, 2)
                }
            ];

            game.DrawableScene3D.AddChild(new Cube3D
            {
                Position = Vector3.Zero,
                Material = prototype,
                Size = new Vector3(5, 3, 1)
            });
        });

        game.Run();
    }
}
