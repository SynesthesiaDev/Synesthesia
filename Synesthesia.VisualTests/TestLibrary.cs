// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Codon.Optionals;
using Common.Bindable;
using Common.Logger;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.VisualTests;

public class TestLibrary(List<VisualTestCategory> categories) : CompositeDrawable2d
{
    private FillFlowContainer2d sidebar = null!;
    private Container2d visualTestScene = null!;

    public readonly Bindable<VisualTest?> CurrentSelectedTest = new(null);

    protected override void OnLoading()
    {
        var childs = new List<Drawable2d>();
        RelativeSizeAxes = Axes.Both;


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
                    ],
                }
            ]
        };

        childs.Add(new DefaultButton
        {
            Size = new Vector2(240, 40),
            Text = "Clear Current Test",
            Scale = new Vector2(0.8f),
            ColorCombination = DefaultEngineColorCombination.RED,
            TextColor = Color.Black,
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            OnClick = () =>
            {
                CurrentSelectedTest.Value = null;
            }
        });

        categories.ForEach(category =>
        {
            childs.Add(new VisualTestCategoryDrawable(category, this)
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Scale = new Vector2(0.8f),
            });
        });

        sidebar.Children = childs;

        Children = [content];

        CurrentSelectedTest.OnValueChange(e =>
        {
            if (e.NewValue != null && e.OldValue == e.NewValue) return;

            foreach (var child in visualTestScene.Children.ToList())
            {
                visualTestScene.RemoveChild(child);
            }

            if (e.NewValue == null)
            {
                visualTestScene.AddChild(new TextDrawable
                {
                    Text = "No Test Selected",
                    Color = Color.White,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                });

                VisualTestRunner.TestConfiguration.CurrentlySelectedTest = Optional.Empty<string>();
            }
            else
            {
                visualTestScene.AddChild(new VisualTestDrawable(e.NewValue));
                VisualTestRunner.TestConfiguration.CurrentlySelectedTest = Optional.Of(e.NewValue.Name);
            }
        });
    }

    protected override void LoadComplete()
    {
        var selectedTest = VisualTestRunner.TestConfiguration.CurrentlySelectedTest;

        // what the fuck why is selectedTest.Value != null true when it's null... (ini parsing issue?)
        // this is an ugly hack
        if (selectedTest.Value != "null")
        {
            Logger.Verbose($"Not null: {selectedTest.Value}");
            foreach (var test in from category in categories from test in category.VisualTests where test.Name == selectedTest.Value! select test)
            {
                CurrentSelectedTest.Value = test;
            }
        }
        else
        {
            CurrentSelectedTest.Value = null;
        }

        base.LoadComplete();
    }
}
