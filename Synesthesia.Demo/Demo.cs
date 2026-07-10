using System.Numerics;
using Synesthesia.Engine;
using Synesthesia.Engine.Components.Two.Default;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Platform.Host;
using Synesthesia.Engine.Util.Bindables;

namespace Synesthesia.Demo;

internal static class Demo
{
    private static DefaultButton? button;
    private static DefaultTextbox? textbox;
    private static DefaultToggle? toggle;

    [STAThread]
    private static void Main(string[] args)
    {
        var windowHost = new SDL3WindowHost();
        var game = new Game(windowHost);
        var toggled = new Bindable<bool>(false);

        game.OnInitialized.Subscribe(_ =>
        {
            game.DrawableScene2D.Children =
            [
                new FillFlowContainer2D
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = Direction.Vertical,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Spacing = 10,
                    Children =
                    [
                        button = new DefaultButton
                        {
                            Size = new Vector2(140, 50),
                            Text = "Testing",
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            ButtonStyle = DefaultButton.Style.Tertiary,
                        },

                        toggle = new DefaultToggle
                        {
                            Size = new Vector2(65, 24),
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Checked = toggled
                        },

                        textbox = new DefaultTextbox
                        {
                            Size = new Vector2(200, 40),
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                        }
                    ]
                },
            ];

            textbox.IsPassword.BindTo(toggled);
        });

        game.Run();
    }
}
