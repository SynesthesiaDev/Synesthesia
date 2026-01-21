using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;

namespace Synesthesia.Demo;

public static class Program
{
    public static void Main(string[] args)
    {
        var game = new Game
        {
        };

        game.RootComposite2d.Children =
        [
            new FillFlowContainer2d
            {
                AutoSizeAxes = Axes.Both,
                Direction = Direction.Vertical,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = 10,
                Children =
                [
                    new FillFlowContainer2d
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Horizontal,
                        Spacing = 5,
                        Children =
                        [
                            new DefaultEngineButton
                            {
                                Size = new Vector2(120, 40),
                                Text = "Disabled :c",
                                Disabled = true
                            },

                            new DefaultEngineButton
                            {
                                Size = new Vector2(120, 40),
                                Text = "Clicky Clack",
                            },

                            new DefaultEngineButton
                            {
                                Size = new Vector2(120, 40),
                                Text = "Click 2",
                                TextColor = Color.Black,
                                ColorCombination = DefaultEngineColorCombination.Accent
                            },
                        ]
                    },
                    
                    new DefaultEngineCheckbox
                    {
                        Text = "Do Stuff",
                        Size = new Vector2(200, 30),
                    },
                    
                    new DefaultEngineCheckbox
                    {
                        Text = "Also disabled :c",
                        Size = new Vector2(200, 30),
                        Disabled = true,
                    },
                    new DefaultEngineTextbox
                    {
                        Size = new Vector2(300, 40),
                    }
                ],
            },
        ];
        
        game.Run();
    }
}