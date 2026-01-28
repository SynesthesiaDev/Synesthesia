// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;
using Codon.Optionals;

namespace Synesthesia.VisualTests;

public class VisualTestsState(Optional<string> currentTest, bool autoRun)
{
    private Optional<string> currentTest = currentTest;
    private bool autoRun = autoRun;

    public Optional<string> CurrentlySelectedTest
    {
        get => currentTest;
        set
        {
            currentTest = value;
            Task.Run(VisualTestRunner.Save);
        }
    }

    public bool RunAutomatically
    {
        get => autoRun;
        set
        {
            autoRun = value;
            Task.Run(VisualTestRunner.Save);
        }
    }

    public static readonly VisualTestsState DEFAULT = new VisualTestsState(Optional.Empty<string>(), false);

    public static readonly StructCodec<VisualTestsState> CODEC = StructCodec.Of
    (
        "currentlySelectedTest", Codecs.String.Optional(), v => v.CurrentlySelectedTest,
        "runAutomatically", Codecs.Boolean, v => v.RunAutomatically,
        (currentlySelectedTest, runAutomatically) => new VisualTestsState(currentlySelectedTest, runAutomatically)
    );
}
