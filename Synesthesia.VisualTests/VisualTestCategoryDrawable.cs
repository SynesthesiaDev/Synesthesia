// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;

namespace Synesthesia.VisualTests;

public class VisualTestCategoryDrawable(VisualTestCategory visualTestCategory, TestLibrary owningLibrary) : CompositeDrawable2d
{
    private FillFlowContainer2d testContainer = null!;
    private bool expanded = false;

    private const int button_width = 240;
    private const int button_height = 40;

    protected override void OnLoading()
    {
        AutoSizeAxes = Axes.Both;
        Children =
        [
            new FillFlowContainer2d
            {
                AutoSizeAxes = Axes.Both,
                Direction = Direction.Vertical,
                Children =
                [
                    new DefaultEngineButton
                    {
                        Size = new Vector2(button_width, button_height),
                        Text = visualTestCategory.Name,
                        OnClick = () =>
                        {
                            testContainer.ScaleTo(expanded ? 0f : 1f, 100, Easing.In);
                            expanded = !expanded;
                        }
                    },

                    testContainer = new FillFlowContainer2d
                    {
                        Scale = new Vector2(0f),
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Vertical,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Spacing = 5f,
                    }
                ]
            },
        ];
    }

    protected override void LoadComplete()
    {
        visualTestCategory.VisualTests.ForEach(visualTest =>
        {
            testContainer.AddChild(new DefaultEngineButton
            {
                Size = new Vector2(button_width, button_height),
                Text = visualTest.Name,
                ColorCombination = DefaultEngineColorCombination.SURFACE1,
                OnClick = () =>
                {
                    owningLibrary.CurrentSelectedTest.Value = visualTest;
                }
            });
        });
        base.LoadComplete();
    }
}
