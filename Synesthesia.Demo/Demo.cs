using System.Numerics;
using Synesthesia.Engine;
using Synesthesia.Engine.Components.Two.Default;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Platform.Host;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Demo;

internal static class Demo
{
    private static DefaultButton? button;

    [STAThread]
    private static void Main(string[] args)
    {
        var windowHost = new SDL3WindowHost();
        var game = new Game(windowHost);

        game.OnInitialized.Subscribe(_ =>
        {
            game.DrawableScene2d.Children =
            [
                // new Text2d
                // {
                //     Text = "hello there",
                //     Origin = Anchor.Centre,
                //     Anchor = Anchor.Centre
                // }
                button = new DefaultButton
                {
                    Size = new Vector2(140, 50),
                    Text = "Testing",
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    OnClick = clickButton,
                    ButtonStyle = DefaultButton.Style.Tertiary,
                    Disabled = true
                }
            ];
        });

        game.Run();
    }

    private static void clickButton()
    {
        button?.ButtonStyle = button.ButtonStyle.Next();
    }
}
