using System.Numerics;
using Synesthesia.Engine;
using Synesthesia.Engine.Components.Three;
using Synesthesia.Engine.Graphics.Materials;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Physics.Three;
using Synesthesia.Utils.Randomness;

namespace Synesthesia.Demo;

internal static class Demo
{
    [STAThread]
    private static void Main(string[] args)
    {
        var game = new GameBuilder().Build();

        game.OnInitialized.Subscribe(_ =>
        {
            game.DrawableScene3D.Children =
            [
                new GameDemo(game)
            ];
        });

        game.Run();
    }

    private class GameDemo(Game game) : CompositeDrawable3D
    {
        private PhysicsContainer3D? physicsContainer;
        private DefaultMaterial orange = null!;

        protected override void OnLoading()
        {
            var prototype = PrototypeMaterial.From(game.TextureResourceStore.Get("Synesthesia.Resources.Textures.Prototype.gray.png"));
            orange = DefaultMaterial.From(game.TextureResourceStore.Get("Synesthesia.Resources.Textures.Prototype.orange.png"));

            base.OnLoading();
            Children =
            [
                new Skybox3D(),
                // new EditorController
                // {
                //     Position = new Vector3(2, 1, 2)
                // },
                new MicrowavablePaimonDrawable
                {
                    Position = new Vector3(2, 2, 0),
                    Scale = new Vector3(0.3f)
                },

                physicsContainer = new PhysicsContainer3D
                {
                    Children =
                    [
                        new PhysicsBody3D
                        {
                            Type = PhysicsType.Static,
                            Size = new Vector3(100, 1, 100),
                            Position = new Vector3(0, -0.5f, 0),
                            Children = [new Cube3D { Material = prototype }]
                        },
                        new FpsController
                        {
                            Position = new Vector3(0, 10f, 0),
                        }
                    ]
                }
            ];
        }

        protected override bool OnKeyDown(KeyboardInputEvent e)
        {
            if (e.Key == Key.B)
            {
                var child = new PhysicsBody3D
                {
                    Type = PhysicsType.Dynamic,
                    Mass = 1f,
                    Size = new Vector3(1, 1, 1),
                    Position = new Vector3(0, 20, 0),
                    Rotation = new Vector3(Rng.RandomFloat(-90, 90), Rng.RandomFloat(-90, 90), Rng.RandomFloat(-90, 90)),
                    Children = [new Cube3D { Material = orange }]
                };

                physicsContainer?.AddChild(child);

                return true;
            }

            return base.OnKeyDown(e);
        }
    }
}
