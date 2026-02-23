using Codon.IniTranscoder;
using Codon.IniTranscoder.Elements;
using Common.Logger;
using Synesthesia.Engine;
using Synesthesia.VisualTests.Tests;

namespace Synesthesia.VisualTests;

internal static class VisualTestRunner
{
    private const string path = "./test_runner.ini";
    public const string WINDOW_TITLE = "Synesthesia Engine | Visual Tests";

    public static VisualTestsState TestConfiguration { get; private set; } = VisualTestsState.DEFAULT;

    public static readonly List<VisualTestCategory> Tests =
    [
        new("Containers", [typeof(ScrollableContainerTest), typeof(ParallaxContainerTest)]),
        new("Audio", [typeof(SimpleAudioTest)]),
        new("Controls", [typeof(SliderTest), typeof(TextboxTest)]),
        new("Animations", [typeof(AnimationTest), typeof(ComplexContainerAnimationTest)]),
        new("Layout", [typeof(AnchorOriginTest), typeof(AutoSizeStackTest), typeof(RelativeSizeMarginTest), typeof(ShowcaseTest)]),
        new("Performance", [typeof(DrawableSpawnPerformanceTest), typeof(AnimationStormPerformanceTest)]),
        new("Texture", [typeof(TextureFillModeTest), typeof(TextureBigTest)]),
        new("Platform", [typeof(WindowActiveStateTest)])
    ];

    public static void Load()
    {
        if (!File.Exists(path))
        {
            File .Create(path) .Close();
            File.WriteAllText(path, VisualTestsState.CODEC.Encode(IniTranscoder.INSTANCE, VisualTestsState.DEFAULT).ToString());
        }
        else
        {
            var text = File.ReadAllText(path);
            var decoded = VisualTestsState.CODEC.Decode(IniTranscoder.INSTANCE, IniSection.Parse(text));
            TestConfiguration = decoded;
        }

        Logger.Verbose("Loaded test runner state file", Logger.Io);
    }

    public static void Save()
    {
        File.WriteAllText(path, VisualTestsState
            .CODEC.Encode(IniTranscoder.INSTANCE, TestConfiguration)
            .ToString());
    }

    private static void Main(string[] args)
    {
        Load();
        var game = new Game();
        game.WindowTitle.Value = WINDOW_TITLE;

        game.DeferredActionQueue.Enqueue(() =>
        {
            game.RootComposite2d.Children = [new TestLibrary(Tests)];
        });

        game.Run();
    }
}
