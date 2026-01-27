// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.VisualTests;

public class TestLibrary : CompositeDrawable2d
{
    private readonly List<VisualTestCategory> categories = [new VisualTestCategory("Testing", []), new VisualTestCategory("Another one", [])];

    protected override void OnLoading()
    {
        var childs = new List<Drawable2d>();
        RelativeSizeAxes = Axes.Both;

        FillFlowContainer2d sidebar;
        Container2d visualTestScene;

        var content = new FillFlowContainer2d
        {
            RelativeSizeAxes = Axes.Both,
            Direction = Direction.Horizontal,
            Children =
            [
                sidebar = new FillFlowContainer2d
                {
                    Direction = Direction.Vertical,
                    RelativeSizeAxes = Axes.Y,
                    Width = 260f * 0.8f,
                    Spacing = 10f,
                    BackgroundColor = Defaults.BACKGROUND0,
                },

                visualTestScene = new BackgroundContainer2d
                {
                    FillRemainingAxes = Axes.Both,
                    BackgroundColor = Color.Black,
                    Children =
                    [
                        new TextDrawable
                        {
                            Text = "No Test Selected",
                            Color = Color.White,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    ],
                }
            ]
        };

        categories.ForEach(category =>
        {
            childs.Add(new VisualTestCategoryDrawable(category)
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Scale = new Vector2(0.8f),
            });
        });

        sidebar.Children = childs;

        Children = [content];
    }
}
