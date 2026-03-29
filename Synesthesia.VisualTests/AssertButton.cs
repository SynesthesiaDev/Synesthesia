// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Logging;

namespace Synesthesia.VisualTests;

public class AssertButton : StepButton
{
    public required StackTrace? CallStack { get; set; }

    public required Func<bool>? Assertion { get; set; }

    public Func<string>? GetFailureMessage { get; set; }

    public string? ExtendedDescription { get; set; }

    public AssertButton()
    {
        Action += checkAssert;
        IdleColor = Color.Orange;
    }

    private void checkAssert()
    {
        if (Assertion != null && Assertion())
            Success();
        else
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(Text);

            if (!string.IsNullOrEmpty(ExtendedDescription))
                builder.Append($" {ExtendedDescription}");

            if (GetFailureMessage != null)
                builder.Append($": {GetFailureMessage()}");

            if (CallStack != null)
            {
                throw ExceptionDispatchInfo.SetRemoteStackTrace(new InvalidOperationException(builder.ToString()), CallStack.ToString());
            }

            Logger.Error(builder.ToString());
        }
    }

    public override void Reset()
    {
        GetFailureMessage = null;
        ExtendedDescription = null;
        Assertion = null;
        CallStack = null;
        OnLoadComplete.Clear();

        base.Reset();
    }
}
