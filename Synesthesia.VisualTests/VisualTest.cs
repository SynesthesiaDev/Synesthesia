// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using Common.Util;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;

namespace Synesthesia.VisualTests;

public abstract class VisualTest : CompositeDrawable2d
{
    public string Name => GetType().Name;

    public readonly StepContainer StepsContainer = new();

    protected void AddWaitUntil(string name, Func<bool> condition, long? timeout = null)
    {
        var button = TestLibrary.AWAIT_BUTTON_POOL.Rent();

        button.Condition = condition;
        button.Name = name;
        button.Timeout = timeout;

        StepsContainer.Add(button);
    }

    protected void AddStep(string name, Action action, bool runNextImmediately = false)
    {

        var button = TestLibrary.STEP_BUTTON_POOL.Rent();
        button.Name = name;
        button.Action = action;
        button.RunNextStepImmediately = runNextImmediately;

        StepsContainer.Add(button);
    }

    protected void AddAssert(string name, Func<bool> assert, string? extendedDescription = null)
    {
        var button = TestLibrary.ASSERT_BUTTON_POOL.Rent();

        button.CallStack = new StackTrace(1, true);
        button.Name = name;
        button.Assertion = assert;
        button.ExtendedDescription = extendedDescription;

        StepsContainer.Add(button);
    }

    protected VisualTest()
    {
        RelativeSizeAxes = Axes.Both;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
    }

    public class StepContainer : CompositeDrawable2d
    {
        protected FillFlowContainer2d StepsContainer = null!;

        private readonly List<StepButton> testSteps = [];

        public IEnumerable<StepButton> TestSteps => testSteps;

        public void Add(StepButton stepButton)
        {
            testSteps.Add(stepButton);
            OnLoadComplete.Subscribe(_ =>
            {
                StepsContainer.AddChild(stepButton);
            });
        }

        protected override void OnLoading()
        {
            RelativeSizeAxes = Axes.Both;
            Children =
            [
                new ScrollableContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollContent =
                    [
                        StepsContainer = new FillFlowContainer2d
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = Direction.Vertical,
                            BackgroundColor = Defaults.BACKGROUND0,
                            Spacing = 5,
                        }
                    ]
                }
            ];

            base.OnLoading();
        }
    }
}
