// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Components.Two.Default;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Graphics.Two.Text;
using Synesthesia.Engine.Util.Bindables;
using Synesthesia.Engine.Util.Pooling;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.VisualTests;

public class TestLibrary(List<VisualTestCategory> categories) : CompositeDrawable2d
{
    private FillFlowContainer2d sidebar = null!;
    private Container2d visualTestScene = null!;
    private Container2d stepContainerContainer = null!;

    public readonly Bindable<VisualTest?> CurrentSelectedTest = new(null);
    public readonly Bindable<bool> RunAutomatically = new(VisualTestRunner.TestConfiguration.RunAutomatically);
    public VisualTest.StepContainer? CurrentStepContainer;

    private DefaultCheckbox defaultCheckbox = null!;

    private int currentStepIndex;
    private StepButton? currentStep;

    public static readonly FastObjectPool<StepButton> STEP_BUTTON_POOL = new(() => new StepButton { Name = string.Empty });
    public static readonly FastObjectPool<AssertButton> ASSERT_BUTTON_POOL = new(() => new AssertButton
    {
        Name = string.Empty,
        CallStack = null,
        Assertion = null
    });
    public static readonly FastObjectPool<WaitUntilButton> AWAIT_BUTTON_POOL = new(() => new WaitUntilButton
    {
        Name = string.Empty,
        Condition = () => true
    });

    public void AutoRunNext()
    {
        if (!RunAutomatically.Value) return;

        if (CurrentStepContainer == null || !CurrentStepContainer.TestSteps.Any()) return;
        if (currentStepIndex >= CurrentStepContainer.TestSteps.Count()) return;

        currentStep = CurrentStepContainer.TestSteps.ToList()[currentStepIndex];

        currentStep!.PerformStep().Then(success =>
        {
            if (!success) return;
            if (currentStep.RunNextStepImmediately)
            {
                currentStepIndex++;
                AutoRunNext();
            }
            else
            {
                Scheduler.Value.Schedule(200, _ =>
                {
                    currentStepIndex++;
                    AutoRunNext();
                });
            }
        });
    }


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
                    Width = 280f * 0.8f,
                    Spacing = 10f,
                    BackgroundColor = Defaults.BACKGROUND1,
                },

                stepContainerContainer = new BackgroundContainer2d
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 260f * 0.8f,
                    BackgroundColor = Defaults.BACKGROUND0,
                },

                visualTestScene = new Container2d
                {
                    Masking = true,
                    FillRemainingAxes = Axes.Both,
                    Children =
                    [
                    ],
                }
            ]
        };

        childs.Add(new Container2d
        {
            AutoSizeAxes = Axes.Both,
            AutoSizePadding = new Vector4(10),
            // BackgroundColor = Defaults.BACKGROUND1,
            // BackgroundCornerRadius = 10,
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Children =
            [
                new FillFlowContainer2d
                {
                    Direction = Direction.Vertical,
                    AutoSizeAxes = Axes.Both,
                    Spacing = 5f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children =
                    [
                        new DefaultButton
                        {
                            Size = new Vector2(240, 40),
                            Text = "Clear Current Test",
                            Scale = new Vector2(0.8f),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            ButtonStyle = DefaultButton.Style.Tertiary,
                            OnClick = () =>
                            {
                                CurrentSelectedTest.Value = null;
                            }
                        },
                        new DefaultButton
                        {
                            Size = new Vector2(240, 40),
                            Text = "Reset Current Test",
                            Scale = new Vector2(0.8f),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            ButtonStyle = DefaultButton.Style.Primary,
                            OnClick = ResetCurrentTest,
                        },
                        defaultCheckbox = new DefaultCheckbox
                        {
                            Checked = RunAutomatically,
                            Size = new Vector2(240, 40),
                            Scale = new Vector2(0.8f),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Run Automatically",
                        }
                    ]
                }
            ]
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

            foreach (var test in visualTestScene.Children.ToList().Filter(p => p is VisualTest).Select(child => (child as VisualTest)!))
            {
                stepContainerContainer.RemoveChild(test.StepsContainer);
                visualTestScene.RemoveChild(test);
                Scheduler.Value.CancelAllTasks();
                currentStepIndex = 0;
                currentStep = null;
            }

            visualTestScene.Children = [];

            if (e.NewValue == null)
            {
                visualTestScene.AddChild(new Text2d
                {
                    Text = "No Test Selected",
                    Color = Color.White,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                });

                VisualTestRunner.TestConfiguration.CurrentlySelectedTest = Optional.Empty<string>();
                // stepContainerContainer.ResizeWidthTo(0f, 250, Easing.OutCubic);
            }
            else
            {
                visualTestScene.AddChild(e.NewValue);
                stepContainerContainer.AddChild(CurrentStepContainer = e.NewValue.StepsContainer);
                VisualTestRunner.TestConfiguration.CurrentlySelectedTest = Optional.Of(e.NewValue.Name);
                if (!e.NewValue.StepsContainer.TestSteps.Any())
                {
                    stepContainerContainer.ResizeWidthTo(0f, 250, Easing.OutCubic);
                }
                else
                {
                    if (stepContainerContainer.Width == 0)
                    {
                        stepContainerContainer.ResizeWidthTo(260f * 0.8f, 250, Easing.OutCubic).Then(() =>
                        {
                            CurrentStepContainer!.OnLoadComplete.Subscribe(_ =>
                            {
                                Scheduler.Value.Schedule(100, _ => AutoRunNext());
                            });
                        });
                    }
                    else
                    {
                        CurrentStepContainer!.OnLoadComplete.Subscribe(_ =>
                        {
                            Scheduler.Value.Schedule(100, _ => AutoRunNext());
                        });
                    }
                }
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
            foreach (var test in from category in categories from test in category.VisualTests where test.Name == selectedTest.Value! select test)
            {
                CurrentSelectedTest.Value = Activator.CreateInstance(test) as VisualTest;
            }
        }
        else
        {
            CurrentSelectedTest.Value = null;
        }

        RunAutomatically.OnValueChange(e =>
        {
            VisualTestRunner.TestConfiguration.RunAutomatically = e.NewValue;
            if (!e.NewValue || e.OldValue == e.NewValue) return;
            if (CurrentSelectedTest.Value == null) return;

            ResetCurrentTest();
        });


        sidebar.Invalidate(Invalidation.All);

        base.LoadComplete();
    }

    public void ResetCurrentTest()
    {
        if(CurrentSelectedTest.Value == null) return;

        var current = CurrentSelectedTest.Value!.GetType();
        CurrentSelectedTest.Value = null;
        CurrentSelectedTest.Value = Activator.CreateInstance(current) as VisualTest;
    }
}
