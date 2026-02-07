// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Shapes;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.VisualTests.Tests;

public class WindowActiveStateTest : VisualTest
{
    private DrawableBox2d activeBox = null!;
    private DrawableBox2d hoveredBox = null!;

    protected override void OnLoading()
    {
        Children =
        [
            new FillFlowContainer2d
            {
                AutoSizeAxes = Axes.Both,
                Direction = Direction.Horizontal,
                Spacing = 10f,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                Children =
                [
                    new Container2d
                    {
                        Size = new Vector2(200, 200),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Children =
                        [
                            activeBox = new DrawableBox2d
                            {
                                RelativeSizeAxes = Axes.Both,
                                Color = Color.DarkGray
                            },
                            new TextDrawable
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Window Is Active",
                                Color = Color.Black,
                            }
                        ]
                    },
                ]
            }
        ];
        base.OnLoading();
    }

    protected override void LoadComplete()
    {
        var game = DependencyContainer.Get<Game>();
        game.WindowsHost.WindowFocused.OnValueChange(e => activeBox.FadeColorTo(e.NewValue ? Defaults.GREEN : Defaults.RED, 50, Easing.In), true);

        base.LoadComplete();
    }
}
