using System.Numerics;
using Common.Util;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Three.Shapes;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

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
                player = new Player
                {
                    Position = new Vector3(5, 5, 5),
                },

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

            game.RootComposite2d.Children =
            [
                new FillFlowContainer2d
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Direction = Direction.Vertical,
                    Spacing = 10f,
                    BackgroundColor = Defaults.BACKGROUND2,
                    AutoSizePadding = new Vector4(10),
                    BackgroundCornerRadius = 10,
                    Scale = new Vector2(0.8f),
                    Position = new Vector2(0, 10),
                    AutoSizeAxes = Axes.Both,
                    Children =
                    [
                        new BackgroundContainer2d
                        {
                            Size = new Vector2(250, 20),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children =
                            [
                                new FrameUpdatableTextDrawable
                                {
                                    UpdateOnDraw = () => $"{player.Velocity.ToPrettyString()}",
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight
                                },
                                new TextDrawable
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = "Velocity:"
                                }
                            ]
                        },
                    ]
                }
            ];

            game.RootComposite3d.ActiveCamera3d = player.Camera;
            game.ConsumesCursor.Value = true;
        });

        game.Run();
    }
}
