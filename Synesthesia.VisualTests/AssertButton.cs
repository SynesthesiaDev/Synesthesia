// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;
using Synesthesia.Engine.Configuration;

namespace Synesthesia.VisualTests;

public class AssertButton : StepButton
{
    public required StackTrace CallStack { get; init; }

    public required Func<bool> Assertion { get; init; }

    public Func<string>? GetFailureMessage { get; init; }

    public string? ExtendedDescription { get; init; }

    public AssertButton()
    {
        Action += checkAssert;
        IdleColor = Defaults.ORANGE;
    }

    private void checkAssert()
    {
        if (Assertion())
            Success();
        else
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(Text);

            if (!string.IsNullOrEmpty(ExtendedDescription))
                builder.Append($" {ExtendedDescription}");

            if (GetFailureMessage != null)
                builder.Append($": {GetFailureMessage()}");

            throw ExceptionDispatchInfo.SetRemoteStackTrace(new InvalidOperationException(builder.ToString()), CallStack.ToString());
        }
    }

    public override void PerformStep(bool userTriggered = false)
    {
        base.PerformStep(userTriggered);
    }
}
