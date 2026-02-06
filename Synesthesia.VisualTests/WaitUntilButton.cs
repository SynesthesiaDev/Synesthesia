// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common;
using Common.Logger;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;

namespace Synesthesia.VisualTests;

public class WaitUntilButton : StepButton
{
    public required Func<bool> Condition;

    public long? Timeout = null;

    public override CompletableFuture<bool> PerformStep(bool userTriggered = false)
    {
        Scheduler.Value.CancelAllTasks();

        Highlight.FadeColorTo(RunningColor, 400, Easing.OutQuad);

        var start = Environment.TickCount64;
        var realText = Text.Text;

        var ticks = 0;
        Scheduler.Value.Repeating(1, task =>
        {
            ticks++;

            if (Timeout != null && Environment.TickCount64 - start >= Timeout)
            {
                Failure();
                Logger.Error($"Test Timed out: {realText} (after {Timeout}ms");
                BackgroundContainer.FlashBackground(Defaults.BACKGROUND3, 50, 150, Easing.In, Easing.Out);
                task.CancellationToken.Cancel();
                return;
            }

            Text.Text = $"{realText} ({ticks})";

            try
            {
                if (!Condition.Invoke()) return;

                Success();
                BackgroundContainer.FlashBackground(Defaults.BACKGROUND3, 50, 150, Easing.In, Easing.Out);
                task.CancellationToken.Cancel();
            }
            catch (Exception exception)
            {
                Failure();
                BackgroundContainer.FlashBackground(Defaults.BACKGROUND3, 50, 150, Easing.In, Easing.Out);
                Logger.Exception(exception, Logger.Runtime);
                task.CancellationToken.Cancel();
            }
        });

        return Future;
    }
}
