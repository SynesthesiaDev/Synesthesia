using Codon.IniTranscoder;
using Codon.IniTranscoder.Elements;
using Common.Logger;
using Synesthesia.Engine;
using Synesthesia.VisualTests.Tests;

namespace Synesthesia.VisualTests;

internal static class VisualTestRunner
{
    private const string path = "./test_runner.ini";

    public static VisualTestsState TestConfiguration { get; private set; } = VisualTestsState.DEFAULT;

    public static readonly List<VisualTestCategory> Tests =
    [
        new VisualTestCategory("Containers", [new ScrollableContainerTest()]),
        new VisualTestCategory("Audio", [new SimpleAudioTest()]),
        new VisualTestCategory("Controls", [new SliderTest()])
    ];

    public static void Load()
    {
        if (!File.Exists(path))
        {
            File .Create(path) .Close();
            File.WriteAllText(path, VisualTestsState.CODEC.Encode(IniTranscoder.Instance, VisualTestsState.DEFAULT).ToString());
        }
        else
        {
            var text = File.ReadAllText(path);
            var decoded = VisualTestsState.CODEC.Decode(IniTranscoder.Instance, IniSection.Parse(text));
            TestConfiguration = decoded;
        }

        Logger.Verbose("Loaded test runner state file", Logger.Io);
    }

    public static void Save()
    {
        File.WriteAllText(path, VisualTestsState
            .CODEC.Encode(IniTranscoder.Instance, TestConfiguration)
            .ToString());
    }

    private static void Main(string[] args)
    {
        Load();
        var game = new Game();

        game.DeferredActionQueue.Enqueue(() =>
        {
            game.RootComposite2d.Children = [new TestLibrary(Tests)];
        });

        game.Run();
    }
}
